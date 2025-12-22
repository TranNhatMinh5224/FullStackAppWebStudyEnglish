using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface ITeacherPackageRepository
    {
        // Lấy tất cả gói giáo viên
        Task<List<TeacherPackage>> GetAllTeacherPackagesAsync();
        
        // Lấy gói giáo viên theo ID
        Task<TeacherPackage?> GetTeacherPackageByIdAsync(int id);
        
        // Thêm gói giáo viên
        Task AddTeacherPackageAsync(TeacherPackage teacherPackage);
        
        // Cập nhật gói giáo viên
        Task UpdateTeacherPackageAsync(TeacherPackage teacherPackage);
        
        // Xóa gói giáo viên
        Task DeleteTeacherPackageAsync(int id);
        
        // Lưu thay đổi
        Task SaveChangesAsync();
        
        // Lấy thông tin gói giáo viên theo ngày
        Task<TeacherPackage?> GetInformationTeacherpackageAsync(int teacherId, DateTime date);
        
        // Lấy thông tin gói giáo viên
        Task<TeacherPackage?> GetInformationTeacherpackage(int teacherId);
    }
}
