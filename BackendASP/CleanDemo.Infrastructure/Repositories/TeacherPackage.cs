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
    }
}