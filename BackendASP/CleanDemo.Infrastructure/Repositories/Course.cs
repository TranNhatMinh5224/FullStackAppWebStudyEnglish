using CleanDemo.Domain.Entities;
using CleanDemo.Domain.Enums;
using CleanDemo.Application.Interface;
using CleanDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanDemo.Infrastructure.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly AppDbContext _context;
        
        public CourseRepository(AppDbContext context)
        {
            _context = context;
        }

        // === CRUD CƠ BẢN ===
        
        /// <summary>
        /// Thêm mới khóa học
        /// </summary>
        public async Task AddCourse(Course course)
        {
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Lấy khóa học theo ID (bao gồm Teacher, Lessons, UserCourses)
        /// </summary>
        public async Task<Course?> GetCourseById(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .Include(c => c.UserCourses)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        /// <summary>
        /// Lấy khóa học với đầy đủ thông tin
        /// </summary>
        public async Task<Course?> GetCourseWithDetails(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Vocabularies)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.MiniTests)
                .Include(c => c.UserCourses)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        /// <summary>
        /// Cập nhật khóa học
        /// </summary>
        public async Task UpdateCourse(Course course)
        {
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Xóa khóa học
        /// </summary>
        public async Task DeleteCourse(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }

        // === LẤY DANH SÁCH KHÓA HỌC ===
        
        /// <summary>
        /// Lấy TẤT CẢ khóa học (Admin)
        /// </summary>
        public async Task<IEnumerable<Course>> GetAllCourses()
        {
            return await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .Include(c => c.UserCourses)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy khóa học hệ thống (User trang chủ)
        /// </summary>
        public async Task<IEnumerable<Course>> GetAllCourseSystem()
        {
            return await _context.Courses
                .Where(c => c.Type == CourseType.System)
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy khóa học do teacher tạo
        /// </summary>
        public async Task<IEnumerable<Course>> GetAllCoursesByTeacherId(int teacherId)
        {
            return await _context.Courses
                .Where(c => c.TeacherId == teacherId && c.Type == CourseType.Teacher)
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .Include(c => c.UserCourses)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy khóa học user đã đăng ký
        /// </summary>
        public async Task<IEnumerable<Course>> GetEnrolledCoursesByUserId(int userId)
        {
            return await _context.UserCourses
                .Where(uc => uc.UserId == userId)
                .Include(uc => uc.Course)
                    .ThenInclude(c => c!.Teacher)
                .Include(uc => uc.Course)
                    .ThenInclude(c => c!.Lessons)
                .Select(uc => uc.Course!)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy khóa học teacher mà user đã tham gia
        /// </summary>
        public async Task<IEnumerable<Course>> GetEnrolledTeacherCoursesByUserId(int userId)
        {
            return await _context.UserCourses
                .Where(uc => uc.UserId == userId && uc.Course!.Type == CourseType.Teacher)
                .Include(uc => uc.Course)
                    .ThenInclude(c => c!.Teacher)
                .Include(uc => uc.Course)
                    .ThenInclude(c => c!.Lessons)
                .Select(uc => uc.Course!)
                .ToListAsync();
        }

        // === KIỂM TRA & ĐĂNG KÝ ===
        
        /// <summary>
        /// Kiểm tra user đã đăng ký khóa học chưa
        /// </summary>
        public async Task<bool> IsUserEnrolledInCourse(int userId, int courseId)
        {
            return await _context.UserCourses
                .AnyAsync(uc => uc.UserId == userId && uc.CourseId == courseId);
        }

        /// <summary>
        /// Đăng ký user vào khóa học
        /// </summary>
        public async Task EnrollUserInCourse(int userId, int courseId)
        {
            var exists = await IsUserEnrolledInCourse(userId, courseId);
            if (exists)
            {
                throw new InvalidOperationException("User already enrolled in this course");
            }

            var enrollment = new UserCourse
            {
                UserId = userId,
                CourseId = courseId,
                JoinedAt = DateTime.UtcNow
            };

            await _context.UserCourses.AddAsync(enrollment);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Hủy đăng ký khóa học
        /// </summary>
        public async Task UnenrollUserFromCourse(int userId, int courseId)
        {
            var enrollment = await _context.UserCourses
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == courseId);

            if (enrollment != null)
            {
                _context.UserCourses.Remove(enrollment);
                await _context.SaveChangesAsync();
            }
        }

        // === THỐNG KÊ ===
        
        /// <summary>
        /// Đếm số lượng bài học
        /// </summary>
        public async Task<int> CountLessons(int courseId)
        {
            return await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .CountAsync();
        }

        /// <summary>
        /// Đếm số user đã đăng ký
        /// </summary>
        public async Task<int> CountEnrolledUsers(int courseId)
        {
            return await _context.UserCourses
                .Where(uc => uc.CourseId == courseId)
                .CountAsync();
        }

        /// <summary>
        /// Lấy danh sách user đã đăng ký
        /// </summary>
        public async Task<IEnumerable<User>> GetEnrolledUsers(int courseId)
        {
            return await _context.UserCourses
                .Where(uc => uc.CourseId == courseId)
                .Include(uc => uc.User)
                .Select(uc => uc.User!)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy khóa học theo ID (alias cho GetCourseById)
        /// </summary>
        public async Task<Course?> GetByIdAsync(int courseId)
        {
            return await GetCourseById(courseId);
        }

        /// <summary>
        /// Lấy khóa học hệ thống (User trang chủ) - alias cho GetAllCourseSystem
        /// </summary>
        public async Task<IEnumerable<Course>> GetSystemCourses()
        {
            return await GetAllCourseSystem();
        }

        /// <summary>
        /// Lấy khóa học do teacher tạo - alias cho GetAllCoursesByTeacherId
        /// </summary>
        public async Task<IEnumerable<Course>> GetCoursesByTeacher(int teacherId)
        {
            return await GetAllCoursesByTeacherId(teacherId);
        }

        /// <summary>
        /// Lấy khóa học user đã đăng ký - alias cho GetEnrolledCoursesByUserId
        /// </summary>
        public async Task<IEnumerable<Course>> GetEnrolledCoursesByUser(int userId)
        {
            return await GetEnrolledCoursesByUserId(userId);
        }

        /// <summary>
        /// Kiểm tra user đã đăng ký khóa học chưa - alias cho IsUserEnrolledInCourse
        /// </summary>
        public async Task<bool> IsUserEnrolled(int courseId, int userId)
        {
            return await IsUserEnrolledInCourse(userId, courseId);
        }
    }
}