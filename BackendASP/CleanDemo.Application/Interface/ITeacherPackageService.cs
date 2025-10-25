using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Interface
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