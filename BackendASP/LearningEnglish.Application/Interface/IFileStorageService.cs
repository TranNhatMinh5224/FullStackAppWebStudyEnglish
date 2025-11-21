using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IFileStorageService
    {
        Task<ServiceResponse<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<ServiceResponse<bool>> DeleteFileAsync(string fileUrl);
        Task<ServiceResponse<Stream>> DownloadFileAsync(string fileUrl);
        Task<ServiceResponse<string>> GetFileUrlAsync(string fileKey);
        Task<ServiceResponse<List<string>>> ListFilesAsync(string prefix = "");
    }
}