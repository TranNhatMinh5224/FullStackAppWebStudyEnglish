using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.Common.Constants;
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
            if (user.Roles.Any(r => r.Name == RoleConstants.Teacher))
            {
                return true; // Already has Teacher role
            }

            // Get Teacher role by name
            var teacherRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == RoleConstants.Teacher);
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

            // Kiểm tra có role Admin không (SuperAdmin, ContentAdmin, FinanceAdmin)
            return user.Roles.Any(r => RoleConstants.IsAdminRole(r.Name));
        }

        // Kiểm tra user có role Teacher trong database
        public async Task<bool> HasTeacherRoleAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return false;
            }

            // Kiểm tra có role Teacher không (check theo tên role để linh hoạt)
            return user.Roles.Any(r => r.Name.Equals(RoleConstants.Teacher, StringComparison.OrdinalIgnoreCase));
        }

        // Lấy danh sách teacher
        public async Task<List<User>> GetAllTeachersAsync()
        {
            var teachers = await _context.Users
                .Include(u => u.Roles)
                .Where(u => u.Roles.Any(r => r.RoleId == 4))
                .ToListAsync();
            return teachers;
        }

        // Lấy danh sách teacher với phân trang, search và sort
        public async Task<PagedResult<User>> GetAllTeachersPagedAsync(UserQueryParameters request)
        {
            var query = _context.Users
                .Include(u => u.Roles)
                .Where(u => u.Roles.Any(r => r.RoleId == 4));

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

        // Lấy user theo khóa học với phân trang cho Teacher (kiểm tra ownership)
        public async Task<PagedResult<User>> GetUsersByCourseIdPagedForTeacherAsync(int courseId, int teacherId, UserQueryParameters request)
        {
            var query = _context.UserCourses
                .Where(uc => uc.CourseId == courseId && uc.User != null)
                .Join(_context.Courses,
                    uc => uc.CourseId,
                    c => c.CourseId,
                    (uc, c) => new { UserCourse = uc, Course = c })
                .Where(x => x.Course.TeacherId == teacherId)
                .Select(x => x.UserCourse.User!)
                .Include(u => u.Roles)
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

        // Lấy dữ liệu chi tiết học sinh trong course cho Teacher (kiểm tra ownership)
        public async Task<(User? Student, UserCourse? UserCourse, CourseProgress? Progress)> GetStudentDetailDataForTeacherAsync(int courseId, int studentId, int teacherId)
        {
            // Kiểm tra course ownership
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId && c.TeacherId == teacherId);
            if (course == null)
            {
                return (null, null, null);
            }

            var student = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == studentId);

            var userCourse = await _context.UserCourses
                .FirstOrDefaultAsync(uc => uc.CourseId == courseId && uc.UserId == studentId);

            var progress = await _context.CourseProgresses
                .FirstOrDefaultAsync(cp => cp.UserId == studentId && cp.CourseId == courseId);

            return (student, userCourse, progress);
        }
    }
}