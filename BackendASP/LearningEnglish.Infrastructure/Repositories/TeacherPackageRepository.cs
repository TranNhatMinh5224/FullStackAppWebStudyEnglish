using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Interface;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace LearningEnglish.Infrastructure.Repositories
{
    public class TeacherPackageRepository : ITeacherPackageRepository
    {
        private readonly AppDbContext _context;

        public TeacherPackageRepository(AppDbContext context)
        {
            _context = context;
        }

        // TeacherPackages là public catalog - không cần RLS
        // Mọi người (Guest, Student, Teacher) đều có thể xem
        // Chỉ Admin mới có thể CRUD (đã có Permission check ở controller)
        public async Task<List<TeacherPackage>> GetAllTeacherPackagesAsync()
        {
            return await _context.TeacherPackages.ToListAsync();
        }

        // TeacherPackages là public catalog - không cần RLS
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

        // Lấy TeacherPackage của teacher tại thời điểm date
        // Tự động lấy subscription đang valid (Active hoặc Pending đã đến ngày)
        // RLS: TeacherSubscriptions đã filter theo userId (Teacher chỉ xem subscriptions của mình, Admin xem tất cả)
        public async Task<TeacherPackage?> GetInformationTeacherpackageAsync(int teacherId, DateTime date)
        {
            // RLS đã filter TeacherSubscriptions theo userId, chỉ cần filter date và status
            var result = await (from tp in _context.TeacherPackages
                                join ts in _context.TeacherSubscriptions on tp.TeacherPackageId equals ts.TeacherPackageId
                                where ts.StartDate <= date
                                      && ts.EndDate >= date
                                      && (ts.Status == SubscriptionStatus.Active || ts.Status == SubscriptionStatus.Pending)
                                orderby ts.EndDate descending
                                select tp).FirstOrDefaultAsync();

            return result;
        }

        // Lấy TeacherPackage hiện tại của teacher
        // Tự động lấy subscription đang valid (Active hoặc Pending đã đến ngày)
        // RLS: TeacherSubscriptions đã filter theo userId (Teacher chỉ xem subscriptions của mình, Admin xem tất cả)
        public async Task<TeacherPackage?> GetInformationTeacherpackage(int teacherId)
        {
            var now = DateTime.UtcNow;
            // RLS đã filter TeacherSubscriptions theo userId, chỉ cần filter date và status
            var result = await (from tp in _context.TeacherPackages
                                join ts in _context.TeacherSubscriptions on tp.TeacherPackageId equals ts.TeacherPackageId
                                where ts.StartDate <= now
                                      && ts.EndDate >= now
                                      && (ts.Status == SubscriptionStatus.Active || ts.Status == SubscriptionStatus.Pending)
                                orderby ts.EndDate descending
                                select tp).FirstOrDefaultAsync();

            return result;
        }
    }
}
