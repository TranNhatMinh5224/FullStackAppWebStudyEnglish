using LearningEnglish.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace LearningEnglish.Application.Interface
{
    public interface IFileStorageService
    {
        Task<UploadTempFileResponseDto> UploadTempFileAsync(IFormFile file);

        Task<ConvertTempToRealFileResponseDto> ConvertTempToRealFileAsync(string tempKey, string realFolderPath);

        Task<bool> DeleteTempFileAsync(string tempKey);

        Task<bool> DeleteRealFileAsync(string fileKey);

        string GetFileUrl(string fileKey);

        Task<bool> FileExistsAsync(string fileKey);
    }
}
