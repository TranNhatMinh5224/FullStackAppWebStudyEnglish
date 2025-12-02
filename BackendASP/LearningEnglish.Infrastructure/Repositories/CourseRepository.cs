using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Interface;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly AppDbContext _context;

        public CourseRepository(AppDbContext context)
        {
            _context = context;
        }


        // Thêm mới khóa học

        public async Task AddCourse(Course course)
        {
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
        }


        // Lấy khóa học theo ID (bao gồm Teacher, Lessons, UserCourses)

        public async Task<Course?> GetCourseById(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .Include(c => c.UserCourses)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }


        //Lấy khóa học với đầy đủ thông tin

        public async Task<Course?> GetCourseWithDetails(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)

                .Include(c => c.UserCourses)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }


        // Cập nhật khóa học

        public async Task UpdateCourse(Course course)
        {
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
        }


        // Xóa khóa học

        public async Task DeleteCourse(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }




        //Lấy TẤT CẢ khóa học (Admin)

        public async Task<IEnumerable<Course>> GetAllCourses()
        {
            return await _context.Courses
                .ToListAsync();
        }


        // Lấy khóa học hệ thống (User trang chủ)

        public async Task<IEnumerable<Course>> GetAllCourseSystem()
        {
            return await _context.Courses
                .Where(c => c.Type == CourseType.System)
                .ToListAsync();
        }


        /// Lấy khóa học do teacher tạo

        public async Task<IEnumerable<Course>> GetAllCoursesByTeacherId(int teacherId)
        {
            return await _context.Courses
                .Where(c => c.TeacherId == teacherId && c.Type == CourseType.Teacher)
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .Include(c => c.UserCourses)
                .ToListAsync();
        }


        /// Lấy khóa học user đã đăng ký

        public async Task<IEnumerable<Course>> GetEnrolledCoursesByUserId(int userId)
        {
            return await _context.UserCourses
                .Where(uc => uc.UserId == userId) // Lọc theo userId
                .Include(uc => uc.Course) // Include khóa học
                    .ThenInclude(c => c!.Teacher) // Include thông tin giáo viên
                .Include(uc => uc.Course) // Include khóa học
                    .ThenInclude(c => c!.Lessons) //    Include bài học
                .Select(uc => uc.Course!) // Lấy danh sách khóa học
                .ToListAsync(); // Trả về danh sách khóa học
        }


        //Lấy khóa học teacher mà user đã tham gia

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




        //Kiểm tra user đã đăng ký khóa học chưa

        public async Task<bool> IsUserEnrolledInCourse(int userId, int courseId)
        {
            return await _context.UserCourses
                .AnyAsync(uc => uc.UserId == userId && uc.CourseId == courseId);
        }


        // Đăng ký user vào khóa học
        public async Task EnrollUserInCourse(int userId, int courseId)
        {
            // Sử dụng transaction để đảm bảo atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var exists = await IsUserEnrolledInCourse(userId, courseId);
                if (exists)
                {
                    throw new InvalidOperationException("User already enrolled in this course");
                }

                // CRITICAL: Sử dụng raw SQL với ROWLOCK + UPDLOCK để lock row trong transaction
                // Đảm bảo không có race condition khi nhiều user cùng enroll
                var course = await _context.Courses
                    .FromSqlRaw("SELECT * FROM \"Courses\" WHERE \"CourseId\" = {0} FOR UPDATE", courseId)
                    .FirstOrDefaultAsync();

                if (course == null)
                {
                    throw new InvalidOperationException("Course not found");
                }

                // Kiểm tra lại sau khi lock - đảm bảo chắc chắn không vượt quá
                if (!course.CanJoin())
                {
                    throw new InvalidOperationException($"Cannot enroll: Course is full ({course.EnrollmentCount}/{course.MaxStudent})");
                }

                // Sử dụng business logic từ Entity (sẽ throw exception nếu đầy)
                course.EnrollStudent();

                var enrollment = new UserCourse
                {
                    UserId = userId,
                    CourseId = courseId,
                    JoinedAt = DateTime.UtcNow
                };

                await _context.UserCourses.AddAsync(enrollment);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        //Hủy đăng ký khóa học
        public async Task UnenrollUserFromCourse(int courseId, int userId)
        {
            // Sử dụng transaction để đảm bảo atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var enrollment = await _context.UserCourses
                    .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == courseId);

                if (enrollment == null)
                {
                    throw new InvalidOperationException("User is not enrolled in this course");
                }

                // CRITICAL: Lock course row để tránh race condition khi nhiều user cùng unenroll
                var course = await _context.Courses
                    .FromSqlRaw("SELECT * FROM \"Courses\" WHERE \"CourseId\" = {0} FOR UPDATE", courseId)
                    .FirstOrDefaultAsync();

                if (course == null)
                {
                    throw new InvalidOperationException("Course not found");
                }

                // Sử dụng business logic từ Entity
                course.UnenrollStudent();

                _context.UserCourses.Remove(enrollment);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }




        // Đếm số lượng bài học

        public async Task<int> CountLessons(int courseId)
        {
            return await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .CountAsync();
        }

        // Đếm số user đã đăng ký

        public async Task<int> CountEnrolledUsers(int courseId)
        {
            return await _context.UserCourses
                .Where(uc => uc.CourseId == courseId)
                .CountAsync();
        }

        // Lấy danh sách user đã đăng ký

        public async Task<IEnumerable<User>> GetEnrolledUsers(int courseId)
        {
            return await _context.UserCourses
                .Where(uc => uc.CourseId == courseId)
                .Include(uc => uc.User)
                .Select(uc => uc.User!)
                .ToListAsync();
        }

        // Lấy tổng số students của teacher (tối ưu - chỉ sum EnrollmentCount)
        public async Task<int> GetTotalStudentsByTeacher(int teacherId)
        {
            return await _context.Courses
                .Where(c => c.TeacherId == teacherId)
                .SumAsync(c => c.EnrollmentCount);
        }


        // Lấy khóa học theo ID (alias cho GetCourseById)

        public async Task<Course?> GetByIdAsync(int courseId)
        {
            return await GetCourseById(courseId);
        }


        // Lấy khóa học hệ thống (User trang chủ) 

        public async Task<IEnumerable<Course>> GetSystemCourses()
        {
            return await GetAllCourseSystem();
        }


        // Lấy khóa học do teacher tạo 

        public async Task<IEnumerable<Course>> GetCoursesByTeacher(int teacherId)
        {
            return await GetAllCoursesByTeacherId(teacherId);
        }


        // Lấy khóa học user đã đăng ký 

        public async Task<IEnumerable<Course>> GetEnrolledCoursesByUser(int userId)
        {
            return await GetEnrolledCoursesByUserId(userId);
        }


        // Kiểm tra user đã đăng ký khóa học chưa 

        public async Task<bool> IsUserEnrolled(int courseId, int userId)
        {
            return await IsUserEnrolledInCourse(userId, courseId);
        }

        // tìm kiem khoa học theo classcode 
        public async Task<IEnumerable<Course>> SearchCourses(string keyword)
        {
            return await _context.Courses
                .Where(c => c.ClassCode == keyword)
                .ToListAsync();

        }

    }
}
