using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.Configurations;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace LearningEnglish.Application.Service
{
    public class FileStorageService : IFileStorageService
    {
        private readonly MinioOptions _minioOptions;
        private readonly IMinioClient _minioClient;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(
            IOptions<MinioOptions> minioOptions,
            IMinioClient minioClient,
            ILogger<FileStorageService> logger)
        {
            _minioOptions = minioOptions.Value;
            _minioClient = minioClient;
            _logger = logger;
        }

        public async Task<ServiceResponse<UploadTempFileResponseDto>> UploadTempFileAsync(IFormFile file)
        {
            var response = new ServiceResponse<UploadTempFileResponseDto>();

            try
            {
                if (file == null || file.Length == 0)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "File không hợp lệ";
                    return response;
                }

                if (file.Length > 10 * 1024 * 1024)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Kích thước file vượt quá giới hạn cho phép (10MB)";
                    return response;
                }

                // Xác định loại file bằng helper
                var fileCategory = FileTypeHelper.GetFileCategory(file.ContentType, file.FileName);
                if (fileCategory == FileCategory.Unknown)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Unsupported file type: {file.ContentType}";
                    return response;
                }

                // Lấy bucket name từ helper
                var bucketName = FileTypeHelper.GetBucketName(fileCategory);

                // Tạo key cho file tạm: temp/{guid}-{filename}
                var uniqueFileName = $"{Guid.NewGuid()}-{file.FileName}";
                var tempKey = $"{FileTypeHelper.GetFolderPath(isTemp: true)}/{uniqueFileName}";
                // → "temp/abc123-image.jpg"

                // Đảm bảo bucket tồn tại
                await EnsureBucketExistsAsync(bucketName);

                // Upload file
                using var stream = file.OpenReadStream();
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(tempKey)
                    .WithStreamData(stream)
                    .WithObjectSize(file.Length)
                    .WithContentType(file.ContentType);

                await _minioClient.PutObjectAsync(putObjectArgs);

                // Tạo preview URL
                var previewUrl = GetFileUrl(bucketName, tempKey); // Private method return string

                // Trả về key với prefix category để dễ xác định bucket sau này
                var fullTempKey = $"{fileCategory.ToString().ToLower()}/temp/{uniqueFileName}";
                // → "image/temp/abc123-image.jpg"

                response.Data = new UploadTempFileResponseDto
                {
                    TempKey = fullTempKey,
                    PreviewUrl = previewUrl,
                    FileName = file.FileName,
                    FileSize = file.Length,
                    ContentType = file.ContentType
                };
                response.StatusCode = 200;
                response.Message = "Upload file thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading temp file");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi upload file";
            }

            return response;
        }

        public async Task<ServiceResponse<ConvertTempToRealFileResponseDto>> ConvertTempToRealFileAsync(string tempKey, string realFolderPath)
        {
            var response = new ServiceResponse<ConvertTempToRealFileResponseDto>();

            try
            {
                if (string.IsNullOrEmpty(tempKey))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "TempKey is required";
                    return response;
                }

                // Extract category từ tempKey bằng helper
                var category = FileTypeHelper.ExtractCategoryFromKey(tempKey);
                var bucketName = FileTypeHelper.GetBucketName(category);

                // Lấy tên file từ tempKey
                // tempKey format: "image/temp/{filename}"
                var fileName = Path.GetFileName(tempKey);
                if (tempKey.Contains("/"))
                {
                    var parts = tempKey.Split('/');
                    fileName = parts[parts.Length - 1];
                }

                // Tạo realKey: real/{folderPath}/{filename}
                var realKey = string.IsNullOrEmpty(realFolderPath)
                    ? $"{FileTypeHelper.GetFolderPath(isTemp: false)}/{fileName}"
                    : $"{FileTypeHelper.GetFolderPath(isTemp: false)}/{realFolderPath}/{fileName}";
                // → "real/courses/123/abc123-image.jpg"

                // Temp key trong bucket (bỏ prefix category)
                var tempKeyInBucket = tempKey.Contains("/temp/") 
                    ? tempKey.Substring(tempKey.IndexOf("/temp/") + 1)  // "temp/{filename}"
                    : tempKey;

                // Kiểm tra file tạm có tồn tại không
                if (!await FileExistsAsync(bucketName, tempKeyInBucket))
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = $"Temp file not found: {tempKey}";
                    return response;
                }

                // Copy từ temp sang real trong cùng bucket
                var copySourceArgs = new CopySourceObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(tempKeyInBucket);

                var copyObjectArgs = new CopyObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(realKey)
                    .WithCopyObjectSource(copySourceArgs);

                await _minioClient.CopyObjectAsync(copyObjectArgs);

                // Xóa file tạm
                var deleteResponse = await DeleteTempFileAsync(tempKey);
                if (!deleteResponse.Success)
                {
                    _logger.LogWarning("Failed to delete temp file: {TempKey}", tempKey);
                }

                // Tạo realKey với prefix category
                var fullRealKey = string.IsNullOrEmpty(realFolderPath)
                    ? $"{category.ToString().ToLower()}/real/{fileName}"
                    : $"{category.ToString().ToLower()}/real/{realFolderPath}/{fileName}";
                // → "image/real/courses/123/abc123-image.jpg"

                var realUrl = GetFileUrl(bucketName, realKey); // Private method return string

                response.Data = new ConvertTempToRealFileResponseDto
                {
                    RealKey = fullRealKey,
                    RealUrl = realUrl
                };
                response.StatusCode = 200;
                response.Message = "Convert file thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting temp to real file: {TempKey}", tempKey);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi convert file";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteTempFileAsync(string tempKey)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                if (string.IsNullOrEmpty(tempKey))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "TempKey is required";
                    response.Data = false;
                    return response;
                }

                // Extract category từ tempKey bằng helper
                var category = FileTypeHelper.ExtractCategoryFromKey(tempKey);
                var bucketName = FileTypeHelper.GetBucketName(category);

                // Lấy key trong bucket (bỏ prefix category)
                var keyInBucket = tempKey.Contains("/temp/") 
                    ? tempKey.Substring(tempKey.IndexOf("/temp/") + 1)
                    : tempKey;

                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(keyInBucket);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);
                
                response.Data = true;
                response.StatusCode = 200;
                response.Message = "Xóa file tạm thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting temp file: {TempKey}", tempKey);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi xóa file tạm";
                response.Data = false;
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteRealFileAsync(string fileKey)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                if (string.IsNullOrEmpty(fileKey))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "FileKey is required";
                    response.Data = false;
                    return response;
                }

                // Extract category từ fileKey bằng helper
                var category = FileTypeHelper.ExtractCategoryFromKey(fileKey);
                var bucketName = FileTypeHelper.GetBucketName(category);

                // Lấy key trong bucket (bỏ prefix category)
                var keyInBucket = fileKey.Contains("/real/") 
                    ? fileKey.Substring(fileKey.IndexOf("/real/") + 1)
                    : fileKey;

                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(keyInBucket);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);
                
                response.Data = true;
                response.StatusCode = 200;
                response.Message = "Xóa file thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting real file: {FileKey}", fileKey);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi xóa file";
                response.Data = false;
            }

            return response;
        }

        public ServiceResponse<string> GetFileUrl(string fileKey)
        {
            var response = new ServiceResponse<string>();

            try
            {
                if (string.IsNullOrEmpty(fileKey))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "FileKey is required";
                    response.Data = string.Empty;
                    return response;
                }

                // Extract category từ fileKey bằng helper
                var category = FileTypeHelper.ExtractCategoryFromKey(fileKey);
                var bucketName = FileTypeHelper.GetBucketName(category);

                // Lấy key trong bucket (bỏ prefix category)
                var keyInBucket = fileKey.Contains("/temp/") || fileKey.Contains("/real/")
                    ? fileKey.Substring(fileKey.IndexOf("/") + 1)  // Bỏ "image/" hoặc "audio/"
                    : fileKey;

                var url = GetFileUrl(bucketName, keyInBucket);
                
                response.Data = url;
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file URL: {FileKey}", fileKey);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi lấy URL file";
                response.Data = string.Empty;
            }

            return response;
        }

        private string GetFileUrl(string bucketName, string keyInBucket)
        {
            try
            {
                var presignedGetObjectArgs = new PresignedGetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(keyInBucket)
                    .WithExpiry(604800); // 7 days

                return _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs).GetAwaiter().GetResult();
            }
            catch
            {
                // Fallback: direct URL
                var baseUrl = _minioOptions.BaseUrl.TrimEnd('/');
                return $"{baseUrl}/{bucketName}/{keyInBucket}";
            }
        }

        public async Task<ServiceResponse<bool>> FileExistsAsync(string fileKey)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                if (string.IsNullOrEmpty(fileKey))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "FileKey is required";
                    response.Data = false;
                    return response;
                }

                // Extract category từ fileKey bằng helper
                var category = FileTypeHelper.ExtractCategoryFromKey(fileKey);
                var bucketName = FileTypeHelper.GetBucketName(category);

                // Lấy key trong bucket
                var keyInBucket = fileKey.Contains("/temp/") || fileKey.Contains("/real/")
                    ? fileKey.Substring(fileKey.IndexOf("/") + 1)
                    : fileKey;

                var exists = await FileExistsAsync(bucketName, keyInBucket);
                
                response.Data = exists;
                response.StatusCode = 200;
                response.Message = exists ? "File tồn tại" : "File không tồn tại";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking file existence: {FileKey}", fileKey);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi kiểm tra file";
                response.Data = false;
            }

            return response;
        }

        private async Task<bool> FileExistsAsync(string bucketName, string keyInBucket)
        {
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(keyInBucket);

                await _minioClient.StatObjectAsync(statObjectArgs);
                return true;
            }
            catch (ObjectNotFoundException)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking file existence: {Bucket}/{Key}", bucketName, keyInBucket);
                return false;
            }
        }

        private async Task EnsureBucketExistsAsync(string bucketName)
        {
            try
            {
                var bucketExistsArgs = new BucketExistsArgs()
                    .WithBucket(bucketName);

                var found = await _minioClient.BucketExistsAsync(bucketExistsArgs);
                
                if (!found)
                {
                    var makeBucketArgs = new MakeBucketArgs()
                        .WithBucket(bucketName);

                    await _minioClient.MakeBucketAsync(makeBucketArgs);
                    _logger.LogInformation("Created bucket: {BucketName}", bucketName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring bucket exists: {BucketName}", bucketName);
                throw;
            }
        }
    }
}
