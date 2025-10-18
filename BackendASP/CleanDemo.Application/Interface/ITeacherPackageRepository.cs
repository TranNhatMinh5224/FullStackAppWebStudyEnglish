using CleanDemo.Domain.Entities;

namespace CleanDemo.Application.Interface
{
    public interface ITeacherPackageRepository
    {
        Task<List<TeacherPackage>> GetAllTeacherPackagesAsync();
        Task<TeacherPackage?> GetTeacherPackageByIdAsync(int id);
        Task AddTeacherPackageAsync(TeacherPackage teacherPackage);
        Task UpdateTeacherPackageAsync(TeacherPackage teacherPackage);
        Task DeleteTeacherPackageAsync(int id);
        Task SaveChangesAsync();
    }
}