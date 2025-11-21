using LearningEnglish.Application.Interface;
using Minio;
using Minio.DataModel.Args;

namespace LearningEnglish.Application.Service
{
    public class MinioFileStorage : IMinioFileStorage
    {
        private readonly IMinioClient _minioClient;

        public MinioFileStorage(IMinioClient minioClient)
        {
            _minioClient = minioClient;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string bucketName, string objectName, string contentType)
        {
            // Ensure bucket exists
            if (!await BucketExistsAsync(bucketName))
            {
                await CreateBucketAsync(bucketName);
            }

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);

            return await GetFileUrlAsync(bucketName, objectName);
        }

        public async Task<bool> DeleteFileAsync(string bucketName, string objectName)
        {
            try
            {
                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Stream> DownloadFileAsync(string bucketName, string objectName)
        {
            var memoryStream = new MemoryStream();

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream));

            await _minioClient.GetObjectAsync(getObjectArgs);

            memoryStream.Position = 0;
            return memoryStream;
        }

        public async Task<string> GetFileUrlAsync(string bucketName, string objectName)
        {
            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithExpiry(60 * 60 * 24 * 7); // 7 days

            return await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
        }

        public Task<List<string>> ListFilesAsync(string bucketName, string prefix = "")
        {
            // TODO: Implement proper Minio list objects functionality
            // For now, return empty list
            return Task.FromResult(new List<string>());
        }

        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            try
            {
                var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
                return await _minioClient.BucketExistsAsync(bucketExistsArgs);
            }
            catch
            {
                return false;
            }
        }

        public async Task CreateBucketAsync(string bucketName)
        {
            var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
            await _minioClient.MakeBucketAsync(makeBucketArgs);
        }
    }
}