using LearningEnglish.Application.DTOs;
using Microsoft.AspNetCore.Http; // for IFormFile
using LearningEnglish.Application.Common;
namespace LearningEnglish.Application.Interface
{
    public interface IMinioFileStorage
    {
        Task<ServiceResponse<ResultUploadDto>> UpLoadFileTempAsync(IFormFile file, string BucketName, string? tempFolder = "temp");
        Task<ServiceResponse<bool>> DeleteFileAsync(string objectKey, string BucketName);
        Task<ServiceResponse<string>> CommitFileAsync(string TempKey, string BucketName, string CommitFolder);
        Task<ServiceResponse<Stream>> DownloadFileAsync(string objectKey, string BucketName);
    }
}