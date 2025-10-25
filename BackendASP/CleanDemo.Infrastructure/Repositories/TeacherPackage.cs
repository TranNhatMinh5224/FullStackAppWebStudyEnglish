using CleanDemo.Domain.Entities;
using CleanDemo.Domain.Enums;
using CleanDemo.Application.Interface;
using CleanDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace CleanDemo.Infrastructure.Repositories
{
    public class TeacherPackageRepository : ITeacherPackageRepository
    {
        private readonly AppDbContext _context;

        public TeacherPackageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TeacherPackage>> GetAllTeacherPackagesAsync()
        {
            return await _context.TeacherPackages.ToListAsync();
        }

        public async Task<TeacherPackage?> GetTeacherPackageByIdAsync(int id)
        {
            return await _context.TeacherPackages.FindAsync(id);
        }

        public async Task AddTeacherPackageAsync(TeacherPackage teacherPackage)
        {
            await _context.TeacherPackages.AddAsync(teacherPackage);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTeacherPackageAsync(TeacherPackage teacherPackage)
        {
            _context.TeacherPackages.Update(teacherPackage);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTeacherPackageAsync(int id)
        {
            var teacherPackage = await _context.TeacherPackages.FindAsync(id);
            if (teacherPackage != null)
            {
                _context.TeacherPackages.Remove(teacherPackage);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Lấy TeacherPackage của teacher tại thời điểm date (có subscription active)
        /// </summary>
        public async Task<TeacherPackage?> GetInformationTeacherpackageAsync(int teacherId, DateTime date)
        {
            var result = await (from tp in _context.TeacherPackages
                         join ts in _context.TeacherSubscriptions on tp.TeacherPackageId equals ts.TeacherPackageId
                         where ts.UserId == teacherId 
                               && ts.StartDate <= date 
                               && ts.EndDate >= date
                               && ts.Status == SubscriptionStatus.Active
                         orderby ts.EndDate descending
                         select tp).FirstOrDefaultAsync();
            
            return result;
        }

        /// <summary>
        /// Lấy TeacherPackage hiện tại của teacher (subscription active)
        /// </summary>
        public async Task<TeacherPackage?> GetInformationTeacherpackage(int teacherId)
        {
            var now = DateTime.UtcNow;
            var result = await (from tp in _context.TeacherPackages
                         join ts in _context.TeacherSubscriptions on tp.TeacherPackageId equals ts.TeacherPackageId
                         where ts.UserId == teacherId 
                               && ts.StartDate <= now 
                               && ts.EndDate >= now
                               && ts.Status == SubscriptionStatus.Active
                         orderby ts.EndDate descending
                         select tp).FirstOrDefaultAsync();
            
            return result;
        }
    }
}