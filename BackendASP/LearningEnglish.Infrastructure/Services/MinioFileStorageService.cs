using LearningEnglish.Application.DTOs;
using Microsoft.AspNetCore.Http; // for IFormFile
using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;
using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using Minio;
using Minio.Exceptions;
using System;
using System.IO;
using System.Threading.Tasks;
using Minio.DataModel.Args; // là namespace của MinIO SDK chứa các class “Args” dùng để cấu hình và gửi lệnh cho MinIO. Tất cả các thao tác của MinIO (upload, copy, delete, check bucket,…) đều cần 1 object Args

namespace LearningEnglish.Infrastructure.MinioFileStorage
{
    public class MinioFileStorageService : IMinioFileStorage
    {
        private readonly IMapper _mapper;
        private readonly IMinioClient _minioClient; // client để kết nối với minio server
        private readonly ILogger<MinioFileStorageService> _logger;

        public MinioFileStorageService(IMapper mapper, IMinioClient minioClient, ILogger<MinioFileStorageService> logger)
        {
            _mapper = mapper;
            _minioClient = minioClient;
            _logger = logger;
        }

        public async Task<ServiceResponse<ResultUploadDto>> UpLoadFileTempAsync(IFormFile file, string BucketName, string? tempFolder = "temp")
        {
            var response = new ServiceResponse<ResultUploadDto>();

            try
            {
                if (file == null || file.Length == 0)
                {
                    response.Success = false;
                    response.Message = "File is null or empty.";
                    response.StatusCode = 400;
                    return response;
                }

                // nếu bucket không tồn tại thì tạo mới
                await EnsureBucketExistsAsync(BucketName);

                // tạo tempkey cho file
                var extension = Path.GetExtension(file.FileName);
                var datetime = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var objectKey = $"{tempFolder}/{datetime}/{Guid.NewGuid()}{extension}";
                objectKey = NormalizeKey(objectKey);


                var imageUrl = BuildPublicUrl.BuildURL(BucketName, objectKey);// gọi helper để tạo url public

                var putfile = new PutObjectArgs() // tạo object args để upload file
                    .WithBucket(BucketName) // tên bucket
                    .WithObject(objectKey) // đặt tên file trên minio server
                    .WithStreamData(file.OpenReadStream()) // lấy stream từ IFormFile
                    .WithObjectSize(file.Length) // kích thước file
                    .WithContentType(file.ContentType); // kiểu file 

                await _minioClient.PutObjectAsync(putfile); // upload file lên minio serverl
                _logger?.LogInformation("Uploaded temp object. Bucket={Bucket}, Key={Key}", BucketName, objectKey);

                response.Data = new ResultUploadDto
                {
                    TempKey = objectKey, // chú ý: property trong DTO nên là TempKey (đúng PascalCase)
                    ImageUrl = imageUrl,
                    ImageType = file.ContentType
                };
                response.Success = true;
                response.Message = "File uploaded successfully.";
                response.StatusCode = 200;
                return response;
            }
            catch (MinioException mEx)
            {
                response.Success = false;
                response.Data = null;
                response.Message = mEx.Message;
                response.StatusCode = 500;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Data = null;
                response.Message = ex.Message;
                response.StatusCode = 500;
            }

            return response;
        }

        // Xóa file bất kỳ trong bucket (temp hoặc real) dựa trên objectKey
        public async Task<ServiceResponse<bool>> DeleteFileAsync(string objectKey, string BucketName)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                if (string.IsNullOrWhiteSpace(objectKey))
                {
                    response.Success = false;
                    response.Data = false;
                    response.Message = "Object key is null or empty.";
                    response.StatusCode = 400;
                    return response;
                }

                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(objectKey);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);
                response.Data = true;
                response.Success = true;
                response.Message = "File deleted successfully.";
                response.StatusCode = 200;
            }
            catch (MinioException mEx)
            {
                response.Success = false;
                response.Data = false;
                response.Message = mEx.Message;
                response.StatusCode = 500;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Data = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
            }

            return response;
        }



        public async Task<ServiceResponse<string>> CommitFileAsync(
            string TempKey,
            string BucketName,
            string CommitFolder = "real")
        {
            var response = new ServiceResponse<string>();

            try
            {
                if (string.IsNullOrWhiteSpace(TempKey))
                {
                    response.Success = false;
                    response.Message = "TempKey is null or empty.";
                    response.StatusCode = 400;
                    return response;
                }

                _logger?.LogInformation("CommitFileAsync called. Bucket={Bucket}, TempKey={TempKey}", BucketName, TempKey);

                // kiểm tra bucket tồn tại
                if (!await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(BucketName)))
                {
                    response.Success = false;
                    response.Message = "Bucket does not exist.";
                    response.StatusCode = 400;
                    return response;
                }

                // copy temp to real 
                // Vd : tempkey = temp/20240615123000/uuid.jpg
                // realkey = real/20240615123000/uuid.jpg

                var fileName = Path.GetFileName(TempKey);  // đầu ra là : uuid.jpg
                var getdirname = Path.GetDirectoryName(TempKey)?.Replace("\\", "/"); // đầu ra là : temp/20240615123000
                var part = getdirname?.Split('/'); // tách chuỗi theo dấu /
                var datetimePart = part != null && part.Length > 1 ? part[1] : ""; // lấy phần datetime
                // var realKey = $"{CommitFolder}/{datetimePart}/{fileName}"; // tạo realkey
                string realKey;
                if (!string.IsNullOrEmpty(datetimePart)) // nếu có phần datetime
                {
                    var datePath = datetimePart; // giữ nguyên định dạng datetimePart
                    realKey = $"{CommitFolder}/{datePath}/{fileName}".Replace("\\", "/");
                }
                else // nếu không có phần datetime
                {
                    realKey = $"{CommitFolder}/{fileName}".Replace("\\", "/");
                }

                realKey = NormalizeKey(realKey);

                // thực hiện copy
                var SourceCopyObj = new CopySourceObjectArgs()  // tạo obj . chức năng của CopySourceObjectArgs là để định nghĩa nguồn (source) của đối tượng (object) mà bạn muốn sao chép trong MinIO là nằm ở đâu
                              .WithBucket(BucketName) // tên bucket nguồn
                                .WithObject(TempKey); // tên object nguồn

                var copyObjectArgs = new CopyObjectArgs() // tạo obj để cấu hình việc copy
                    .WithBucket(BucketName) // tên bucket đích
                    .WithObject(realKey) // tên object đích
                    .WithCopyObjectSource(SourceCopyObj);

                _logger?.LogInformation("Copying object from temp to real. Source={Source}, Destination={Dest}", TempKey, realKey);
                await _minioClient.CopyObjectAsync(copyObjectArgs); // thực hiện copy

                // trả về realkey
                response.Data = realKey;
                response.Success = true;
                response.Message = "File committed successfully.";
                response.StatusCode = 200;

                // xóa file temp sau khi copy thành công
                try
                {
                    var removeObjectArgs = new RemoveObjectArgs()
                        .WithBucket(BucketName)
                        .WithObject(TempKey);

                    await _minioClient.RemoveObjectAsync(removeObjectArgs); // thực hiện xóa
                }
                catch
                {
                    // có thể log warning ở đây nếu cần
                }
            }
            catch (MinioException mEx)
            {
                _logger?.LogError(mEx, "MinIO exception during CommitFileAsync. Bucket={Bucket}, TempKey={TempKey}", BucketName, TempKey);
                response.Success = false;
                response.Data = null;
                response.Message = mEx.Message;
                response.StatusCode = 500;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Exception during CommitFileAsync. Bucket={Bucket}, TempKey={TempKey}", BucketName, TempKey);
                response.Success = false;
                response.Data = null;
                response.Message = ex.Message;
                response.StatusCode = 500;
            }

            return response;
        }



        // Download file từ MinIO và trả về Stream
        public async Task<ServiceResponse<Stream>> DownloadFileAsync(string objectKey, string BucketName)
        {
            var response = new ServiceResponse<Stream>();

            try
            {
                if (string.IsNullOrWhiteSpace(objectKey))
                {
                    response.Success = false;
                    response.Message = "Object key is null or empty.";
                    response.StatusCode = 400;
                    return response;
                }

                // Kiểm tra bucket tồn tại
                var bucketExists = await _minioClient.BucketExistsAsync(
                    new BucketExistsArgs().WithBucket(BucketName));

                if (!bucketExists)
                {
                    response.Success = false;
                    response.Message = "Bucket does not exist.";
                    response.StatusCode = 404;
                    return response;
                }

                // Tạo MemoryStream để lưu file data
                var memoryStream = new MemoryStream();

                // Download file từ MinIO
                var getObjectArgs = new GetObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(objectKey)
                    .WithCallbackStream(async (stream) =>
                    {
                        await stream.CopyToAsync(memoryStream);
                    });

                await _minioClient.GetObjectAsync(getObjectArgs);

                // Reset position về đầu stream để có thể đọc
                memoryStream.Position = 0;

                _logger?.LogInformation("Downloaded file successfully. Bucket={Bucket}, Key={Key}", BucketName, objectKey);

                response.Data = memoryStream;
                response.Success = true;
                response.Message = "File downloaded successfully.";
                response.StatusCode = 200;
            }
            catch (MinioException mEx)
            {
                _logger?.LogError(mEx, "MinIO exception during DownloadFileAsync. Bucket={Bucket}, Key={Key}", BucketName, objectKey);
                response.Success = false;
                response.Data = null;
                response.Message = mEx.Message;
                response.StatusCode = 500;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Exception during DownloadFileAsync. Bucket={Bucket}, Key={Key}", BucketName, objectKey);
                response.Success = false;
                response.Data = null;
                response.Message = ex.Message;
                response.StatusCode = 500;
            }

            return response;
        }

        private async Task EnsureBucketExistsAsync(string bucketName)
        {
            var exists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucketName));

            if (!exists)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(bucketName));
            }
        }

        private static string NormalizeKey(string key)
        {
            return key.Replace("\\", "/");
        }
    }
}
