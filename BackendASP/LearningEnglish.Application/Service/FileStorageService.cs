using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;

namespace LearningEnglish.Application.Service
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IMinioFileStorage _minioFileStorage;
        private const string DefaultBucket = "course-files";

        public FileStorageService(IMinioFileStorage minioFileStorage)
        {
            _minioFileStorage = minioFileStorage;
        }

        public async Task<ServiceResponse<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                // Generate unique object name
                var objectName = $"{Guid.NewGuid()}_{fileName}";

                var url = await _minioFileStorage.UploadFileAsync(fileStream, DefaultBucket, objectName, contentType);

                return new ServiceResponse<string>
                {
                    Success = true,
                    Data = url,
                    Message = "File uploaded successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = $"Failed to upload file: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> DeleteFileAsync(string fileUrl)
        {
            try
            {
                // Extract object name from URL
                var objectName = ExtractObjectNameFromUrl(fileUrl);
                if (string.IsNullOrEmpty(objectName))
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid file URL"
                    };
                }

                var result = await _minioFileStorage.DeleteFileAsync(DefaultBucket, objectName);

                return new ServiceResponse<bool>
                {
                    Success = result,
                    Data = result,
                    Message = result ? "File deleted successfully" : "Failed to delete file"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to delete file: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<Stream>> DownloadFileAsync(string fileUrl)
        {
            try
            {
                var objectName = ExtractObjectNameFromUrl(fileUrl);
                if (string.IsNullOrEmpty(objectName))
                {
                    return new ServiceResponse<Stream>
                    {
                        Success = false,
                        Message = "Invalid file URL"
                    };
                }

                var stream = await _minioFileStorage.DownloadFileAsync(DefaultBucket, objectName);

                return new ServiceResponse<Stream>
                {
                    Success = true,
                    Data = stream,
                    Message = "File downloaded successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Stream>
                {
                    Success = false,
                    Message = $"Failed to download file: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<string>> GetFileUrlAsync(string fileKey)
        {
            try
            {
                var url = await _minioFileStorage.GetFileUrlAsync(DefaultBucket, fileKey);

                return new ServiceResponse<string>
                {
                    Success = true,
                    Data = url,
                    Message = "File URL generated successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = $"Failed to generate file URL: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<List<string>>> ListFilesAsync(string prefix = "")
        {
            try
            {
                var files = await _minioFileStorage.ListFilesAsync(DefaultBucket, prefix);

                return new ServiceResponse<List<string>>
                {
                    Success = true,
                    Data = files,
                    Message = $"Found {files.Count} files"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<string>>
                {
                    Success = false,
                    Message = $"Failed to list files: {ex.Message}"
                };
            }
        }

        private string? ExtractObjectNameFromUrl(string fileUrl)
        {
            // This is a simple implementation - in production, you might need more sophisticated URL parsing
            try
            {
                var uri = new Uri(fileUrl);
                var path = uri.AbsolutePath;
                // Remove leading slash and bucket name
                if (path.StartsWith("/"))
                    path = path.Substring(1);

                var parts = path.Split('/');
                if (parts.Length >= 2)
                {
                    // Skip bucket name and return the object name
                    return string.Join("/", parts.Skip(1));
                }

                return path;
            }
            catch
            {
                return null;
            }
        }
    }
}