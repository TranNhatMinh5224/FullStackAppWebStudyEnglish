using LearningEnglish.Application.DTOs;
using Microsoft.AspNetCore.Http; // for IFormFile
using LearningEnglish.Application.Common;
namespace LearningEnglish.Application.Interface
{
    public interface IMinioFileStorage
    {
        // Upload file tạm
        Task<ServiceResponse<ResultUploadDto>> UpLoadFileTempAsync(IFormFile file, string BucketName, string? tempFolder = "temp");
        
        // Xóa file
        Task<ServiceResponse<bool>> DeleteFileAsync(string objectKey, string BucketName);
        
        // Commit file từ temp về folder chính
        Task<ServiceResponse<string>> CommitFileAsync(string TempKey, string BucketName, string CommitFolder);
        
        // Tải file
        Task<ServiceResponse<Stream>> DownloadFileAsync(string objectKey, string BucketName);
    }
}