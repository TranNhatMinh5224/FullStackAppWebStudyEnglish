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
        private readonly ISortingService<User> _sortingService;

        public UserRepository(AppDbContext context, ISortingService<User> sortingService)
        {
            _context = context;
            _sortingService = sortingService;
        }

        public async Task<User?> GetByIdAsync(int id) =>
            await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.UserId == id);

        public async Task<User?> GetUserByEmailAsync(string email) =>
            await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> GetUserByPhoneNumberAsync(string phoneNumber) =>
            await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

        public async Task<List<User>> GetAllUsersAsync() =>
            await _context.Users.Include(u => u.Roles).ToListAsync();

        public async Task<List<User>> GetUsersByRoleAsync(string roleName) =>
            await _context.Users
                .Include(u => u.Roles)
                .Where(u => u.Roles.Any(r => r.Name == roleName))
                .ToListAsync();

        // Lấy tất cả người dùng với phân trang, search và sort
        public async Task<PagedResult<User>> GetAllUsersPagedAsync(UserQueryParameters request)
        {
            var query = _context.Users
                .Include(u => u.Roles)
                .AsQueryable();

            // Apply search filter (case-insensitive, chỉ search theo Email)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(u => u.Email.ToLower().Contains(term));
            }

            // Apply sorting
            query = _sortingService.ApplySort(query, request.SortBy, request.SortOrder);

            return await query.ToPagedListAsync(request.PageNumber, request.PageSize);
        }

        // Lấy danh sách người dùng theo khóa học với phân trang
        public async Task<PagedResult<User>> GetUsersByCourseIdPagedAsync(int courseId, UserQueryParameters request)
        {
            var query = _context.UserCourses
                .Where(uc => uc.CourseId == courseId && uc.User != null)
                .Include(uc => uc.User!)
                    .ThenInclude(u => u.Roles)
                .Select(uc => uc.User!)
                .OrderBy(u => u.FirstName)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(u =>
                    (u.FirstName + " " + u.LastName).ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term));
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
                return true; // Already has Teacher role
            }

            // Get Teacher role (RoleId = 2)
            var teacherRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == 2);
            if (teacherRole == null)
            {
                throw new InvalidOperationException("Teacher role not found in database");
            }

            // Add Teacher role to user (will be saved by caller's SaveChanges)
            user.Roles.Add(teacherRole);
            
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

        // Lấy danh sách teacher với phân trang, search và sort
        public async Task<PagedResult<User>> GetAllTeachersPagedAsync(UserQueryParameters request)
        {
            var query = _context.Users
                .Include(u => u.Roles)
                .Where(u => u.Roles.Any(r => r.RoleId == 2));

            // Apply search filter (case-insensitive)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(u => 
                    (u.FirstName != null && u.FirstName.ToLower().Contains(term)) || 
                    (u.LastName != null && u.LastName.ToLower().Contains(term)) ||
                    u.Email.ToLower().Contains(term)
                );
            }

            // Apply sorting
            query = _sortingService.ApplySort(query, request.SortBy, request.SortOrder);

            return await query.ToPagedListAsync(request.PageNumber, request.PageSize);
        }

        // Lấy danh sách tài khoản bị khóa với phân trang, search và sort
        public async Task<PagedResult<User>> GetListBlockedAccountsPagedAsync(UserQueryParameters request)
        {
            var query = _context.Users
                .Include(u => u.Roles)
                .Where(u => u.Status == AccountStatus.Inactive);

            // Apply search filter (case-insensitive)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(u => 
                    (u.FirstName != null && u.FirstName.ToLower().Contains(term)) || 
                    (u.LastName != null && u.LastName.ToLower().Contains(term)) ||
                    u.Email.ToLower().Contains(term)
                );
            }

            // Apply sorting
            query = _sortingService.ApplySort(query, request.SortBy, request.SortOrder);

            return await query.ToPagedListAsync(request.PageNumber, request.PageSize);
        }

        // Statistics methods cho Admin Dashboard
        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetUserCountByRoleAsync(string roleName)
        {
            return await _context.Users
                .Include(u => u.Roles)
                .Where(u => u.Roles.Any(r => r.Name == roleName))
                .CountAsync();
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            return await _context.Users
                .Where(u => u.Status == AccountStatus.Active)
                .CountAsync();
        }

        public async Task<int> GetBlockedUsersCountAsync()
        {
            return await _context.Users
                .Where(u => u.Status == AccountStatus.Suspended || u.Status == AccountStatus.Inactive)
                .CountAsync();
        }

        public async Task<int> GetNewUsersCountAsync(DateTime fromDate)
        {
            return await _context.Users
                .Where(u => u.CreatedAt >= fromDate)
                .CountAsync();
        }
    }
}