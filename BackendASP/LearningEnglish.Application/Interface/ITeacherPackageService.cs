using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface ITeacherPackageService
    {
        Task<ServiceResponse<List<TeacherPackageDto>>> GetAllTeacherPackagesAsync();
        Task<ServiceResponse<TeacherPackageDto>> GetTeacherPackageByIdAsync(int id);
        Task<ServiceResponse<TeacherPackageDto>> CreateTeacherPackageAsync(CreateTeacherPackageDto dto);
        Task<ServiceResponse<TeacherPackageDto>> UpdateTeacherPackageAsync(int id, UpdateTeacherPackageDto dto);
        Task<ServiceResponse<bool>> DeleteTeacherPackageAsync(int id);
    }
}
