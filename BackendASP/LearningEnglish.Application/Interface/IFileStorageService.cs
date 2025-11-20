using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace LearningEnglish.Application.Interface
{
    public interface IFileStorageService
    {
        Task<ServiceResponse<UploadTempFileResponseDto>> UploadTempFileAsync(IFormFile file);

        Task<ServiceResponse<ConvertTempToRealFileResponseDto>> ConvertTempToRealFileAsync(string tempKey, string realFolderPath);

        Task<ServiceResponse<bool>> DeleteTempFileAsync(string tempKey);

        Task<ServiceResponse<bool>> DeleteRealFileAsync(string fileKey);

        Task<ServiceResponse<string>> GetFileUrl(string fileKey);

        Task<ServiceResponse<bool>> FileExistsAsync(string fileKey);
    }
}
