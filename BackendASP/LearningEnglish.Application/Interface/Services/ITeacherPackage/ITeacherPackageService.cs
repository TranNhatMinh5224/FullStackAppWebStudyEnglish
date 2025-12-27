using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface.Services.TeacherPackage
{
    public interface ITeacherPackageService
    {
        // Lấy danh sách gói giáo viên
        Task<ServiceResponse<List<TeacherPackageDto>>> GetAllTeacherPackagesAsync();
        
        // Lấy thông tin gói giáo viên
        Task<ServiceResponse<TeacherPackageDto>> GetTeacherPackageByIdAsync(int id);
        
        // Tạo gói giáo viên
        Task<ServiceResponse<TeacherPackageDto>> CreateTeacherPackageAsync(CreateTeacherPackageDto dto);
        
        // Cập nhật gói giáo viên
        Task<ServiceResponse<TeacherPackageDto>> UpdateTeacherPackageAsync(int id, UpdateTeacherPackageDto dto);
        
        // Xóa gói giáo viên
        Task<ServiceResponse<bool>> DeleteTeacherPackageAsync(int id);
    }
}
