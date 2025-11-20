using Microsoft.AspNetCore.Http;

namespace LearningEnglish.Application.Interface
{
    public interface IMinioFileStorage
    {
        Task UploadObjectAsync(string bucketName, string objectName, Stream data, long size, string contentType);
        Task CopyObjectAsync(string bucketName, string sourceObjectName, string destObjectName);
        Task RemoveObjectAsync(string bucketName, string objectName);
        Task<bool> ObjectExistsAsync(string bucketName, string objectName);
        Task EnsureBucketExistsAsync(string bucketName);
        Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expirySeconds = 604800);
    }
}
