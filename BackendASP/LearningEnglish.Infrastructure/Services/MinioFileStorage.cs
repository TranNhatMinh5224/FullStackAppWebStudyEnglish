using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LearningEnglish.Application.Configurations;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace LearningEnglish.Infrastructure.Services
{
    public class MinioFileStorage : IMinioFileStorage
    {
        private readonly IMinioClient _minioClient;
        private readonly ILogger<MinioFileStorage> _logger;

        public MinioFileStorage(IMinioClient minioClient, ILogger<MinioFileStorage> logger)
        {
            _minioClient = minioClient;
            _logger = logger;
        }

        public async Task UploadObjectAsync(string bucketName, string objectName, Stream data, long size, string contentType)
        {
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(data)
                .WithObjectSize(size)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);
        }

        public async Task CopyObjectAsync(string bucketName, string sourceObjectName, string destObjectName)
        {
            var copySourceArgs = new CopySourceObjectArgs()
                .WithBucket(bucketName)
                .WithObject(sourceObjectName);

            var copyObjectArgs = new CopyObjectArgs()
                .WithBucket(bucketName)
                .WithObject(destObjectName)
                .WithCopyObjectSource(copySourceArgs);

            await _minioClient.CopyObjectAsync(copyObjectArgs);
        }

        public async Task RemoveObjectAsync(string bucketName, string objectName)
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);

            await _minioClient.RemoveObjectAsync(removeObjectArgs);
        }

        public async Task<bool> ObjectExistsAsync(string bucketName, string objectName)
        {
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName);

                await _minioClient.StatObjectAsync(statObjectArgs);
                return true;
            }
            catch (ObjectNotFoundException)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking object existence: {Bucket}/{Object}", bucketName, objectName);
                return false;
            }
        }

        public async Task EnsureBucketExistsAsync(string bucketName)
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

        public async Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expirySeconds = 604800)
        {
            try
            {
                var presignedGetObjectArgs = new PresignedGetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithExpiry(expirySeconds);

                return await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating presigned URL: {Bucket}/{Object}", bucketName, objectName);
                throw;
            }
        }
    }
}
