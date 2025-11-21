namespace LearningEnglish.Application.Interface
{
    public interface IMinioFileStorage
    {
        Task<string> UploadFileAsync(Stream fileStream, string bucketName, string objectName, string contentType);
        Task<bool> DeleteFileAsync(string bucketName, string objectName);
        Task<Stream> DownloadFileAsync(string bucketName, string objectName);
        Task<string> GetFileUrlAsync(string bucketName, string objectName);
        Task<List<string>> ListFilesAsync(string bucketName, string prefix = "");
        Task<bool> BucketExistsAsync(string bucketName);
        Task CreateBucketAsync(string bucketName);
    }
}