using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context) => _context = context;

        public async Task<User?> GetByIdAsync(int id) =>
            await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.UserId == id);

        public async Task<User?> GetUserByEmailAsync(string email) =>
            await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> GetUserByPhoneNumberAsync(string phoneNumber) =>
            await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

        public async Task<List<User>> GetAllUsersAsync() =>
            await _context.Users.Include(u => u.Roles).ToListAsync();

        // Lấy tất cả người dùng với phân trang
        public async Task<PagedResult<User>> GetAllUsersPagedAsync(PageRequest request)
        {
            var query = _context.Users
                .Include(u => u.Roles)
                .OrderBy(u => u.FirstName)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(term) ||
                    u.FirstName.ToLower().Contains(term) ||
                    u.LastName.ToLower().Contains(term));
            }

            return await query.ToPagedListAsync(request.PageNumber, request.PageSize);
        }

        // Lấy danh sách người dùng theo khóa học với phân trang
        public async Task<PagedResult<User>> GetUsersByCourseIdPagedAsync(int courseId, PageRequest request)
        {
            var query = _context.UserCourses
                .Where(uc => uc.CourseId == courseId && uc.User != null)
                .Select(uc => uc.User!)
                .Include(u => u.Roles)
                .OrderBy(u => u.FirstName)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(u =>
                    (u.FirstName + " " + u.LastName).Contains(request.SearchTerm) ||
                    u.Email.Contains(request.SearchTerm));
            }

            return await query.ToPagedListAsync(request.PageNumber, request.PageSize);
        }

        public async Task AddUserAsync(User user) => await _context.Users.AddAsync(user);

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await Task.CompletedTask;
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user != null) _context.Users.Remove(user);
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<Role?> GetRoleByNameAsync(string roleName) =>
            await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        public async Task<bool> UpdateRoleTeacher(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return false;
            }

            // Check if user already has Teacher role
            if (user.Roles.Any(r => r.RoleId == 2))
            {
                return true;
            }

            // Get Teacher role
            var teacherRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == 2);
            if (teacherRole != null)
            {
                user.Roles.Add(teacherRole);
                await _context.SaveChangesAsync();
            }

            return true;
        }
        // Implement cho phương thức lấy role theo userId
        public async Task<bool> GetUserRolesAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return false;
            }

            // Kiểm tra có role Admin không
            return user.Roles.Any(r => r.Name.Equals("admin", StringComparison.CurrentCultureIgnoreCase));
        }

        // Lấy danh sách teacher
        public async Task<List<User>> GetAllTeachersAsync()
        {
            var teachers = await _context.Users
                .Include(u => u.Roles)
                .Where(u => u.Roles.Any(r => r.RoleId == 2))
                .ToListAsync();
            return teachers;
        }

        // Lấy danh sách teacher với phân trang
        public async Task<PagedResult<User>> GetAllTeachersPagedAsync(PageRequest request)
        {
            var query = _context.Users
                .Include(u => u.Roles)
                .Where(u => u.Roles.Any(r => r.RoleId == 2));

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(u => 
                    u.FirstName.Contains(request.SearchTerm) || 
                    u.LastName.Contains(request.SearchTerm) ||
                    u.Email.Contains(request.SearchTerm)
                );
            }

            return await query.ToPagedListAsync(request.PageNumber, request.PageSize);
        }

        // Lấy danh sách tài khoản bị khóa với phân trang
        public async Task<PagedResult<User>> GetListBlockedAccountsPagedAsync(PageRequest request)
        {
            var query = _context.Users
                .Include(u => u.Roles)
                .Where(u => u.Status == AccountStatus.Inactive);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(u => 
                    u.FirstName.Contains(request.SearchTerm) || 
                    u.LastName.Contains(request.SearchTerm) ||
                    u.Email.Contains(request.SearchTerm)
                );
            }

            return await query.ToPagedListAsync(request.PageNumber, request.PageSize);
        }
    }
}
