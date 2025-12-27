using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.Common.Specifications;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Infrastructure.Data;
using LearningEnglish.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    
    public class CourseRepository : ICourseRepository
    {
        private readonly AppDbContext _context;
        private readonly ISortingService<Course> _sortingService;

        public CourseRepository(AppDbContext context, ISortingService<Course> sortingService)
        {
            _context = context;
            _sortingService = sortingService;
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

        // Lấy tất cả khóa học với phân trang - cho Admin (sort theo Title mặc định)
        public async Task<PagedResult<Course>> GetAllCoursesPagedForAdminAsync(AdminCourseQueryParameters parameters)
        {
            var query = _context.Courses.AsQueryable();

            // Apply includes
            query = query.Include(c => c.Teacher);

            // Apply filters
            if (parameters.Type.HasValue)
            {
                query = query.Where(c => c.Type == parameters.Type.Value);
            }

            if (parameters.Status.HasValue)
            {
                query = query.Where(c => c.Status == parameters.Status.Value);
            }

            if (parameters.IsFeatured.HasValue)
            {
                query = query.Where(c => c.IsFeatured == parameters.IsFeatured.Value);
            }

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var term = parameters.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.Title.ToLower().Contains(term) ||
                    (c.ClassCode != null && c.ClassCode.ToLower().Contains(term)) ||
                    (c.Teacher != null && 
                     (c.Teacher.FirstName.ToLower().Contains(term) || 
                      c.Teacher.LastName.ToLower().Contains(term) ||
                      (c.Teacher.FirstName + " " + c.Teacher.LastName).ToLower().Contains(term))));
            }

            // Sort theo Title mặc định (không có sortBy/sortOrder)
            query = query.OrderBy(c => c.Title);

            return await query.ToPagedListAsync(parameters.PageNumber, parameters.PageSize);
        }

        // Lấy khóa học hệ thống (User trang chủ)
        public async Task<IEnumerable<Course>> GetAllCourseSystem()
        {
            return await _context.Courses
                .Where(c => c.Type == CourseType.System)
                .ToListAsync();
        }


        /// Lấy khóa học do teacher tạo - RLS đã filter theo teacherId

        public async Task<IEnumerable<Course>> GetAllCoursesByTeacherId()
        {
            return await _context.Courses
                .Where(c => c.Type == CourseType.Teacher) // Chỉ filter Type, RLS đã filter TeacherId
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .Include(c => c.UserCourses)
                .ToListAsync();
        }

        // Lấy khóa học của giáo viên với phân trang (chỉ phân trang, không filter) - RLS đã filter theo teacherId
        public async Task<PagedResult<Course>> GetCoursesByTeacherPagedAsync(PageRequest request)
        {
            var query = _context.Courses
                .Where(c => c.Type == CourseType.Teacher) // Chỉ filter Type, RLS đã filter TeacherId
                .Include(c => c.Teacher)
                .OrderBy(c => c.Title) // Sort theo Title mặc định
                .AsQueryable();

            return await query.ToPagedListAsync(request.PageNumber, request.PageSize);
        }


        /// Lấy khóa học user đã đăng ký - RLS đã filter theo userId

        public async Task<IEnumerable<Course>> GetEnrolledCoursesByUserId()
        {
            return await _context.UserCourses
                // RLS đã filter theo userId, không cần .Where(uc => uc.UserId == userId)
                .Include(uc => uc.Course) // Include khóa học
                    .ThenInclude(c => c!.Teacher) // Include thông tin giáo viên
                .Include(uc => uc.Course) // Include khóa học
                    .ThenInclude(c => c!.Lessons) //    Include bài học
                .Select(uc => uc.Course!) // Lấy danh sách khóa học
                .ToListAsync(); // Trả về danh sách khóa học
        }


        // Lấy khóa học teacher mà user đã tham gia - RLS đã filter theo userId
        public async Task<IEnumerable<Course>> GetEnrolledTeacherCoursesByUserId()
        {
            return await _context.UserCourses
                .Where(uc => uc.Course!.Type == CourseType.Teacher) // Chỉ filter Type, RLS đã filter userId
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

            var exists = await IsUserEnrolledInCourse(userId, courseId);
            if (exists)
            {
                throw new InvalidOperationException("User already enrolled in this course");
            }

            // Reload course từ DB để lấy EnrollmentCount mới nhất (tránh race condition)
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                throw new InvalidOperationException("Course not found");
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
        }


        //Hủy đăng ký khóa học
        public async Task UnenrollUserFromCourse(int courseId, int userId)
        {
            var enrollment = await _context.UserCourses
    .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == courseId);

            if (enrollment != null)
            {
                // Lấy course để cập nhật EnrollmentCount
                var course = await _context.Courses.FindAsync(courseId);
                if (course != null)
                {
                    // Sử dụng business logic từ Entity
                    course.UnenrollStudent();
                }

                _context.UserCourses.Remove(enrollment);
                await _context.SaveChangesAsync();
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




        // Lấy khóa học hệ thống (User trang chủ) 

        public async Task<IEnumerable<Course>> GetSystemCourses()
        {
            return await GetAllCourseSystem();
        }


        // Lấy khóa học do teacher tạo - RLS đã filter theo teacherId

        public async Task<IEnumerable<Course>> GetCoursesByTeacher()
        {
            return await GetAllCoursesByTeacherId();
        }


        // Lấy khóa học user đã đăng ký - RLS đã filter theo userId

        public async Task<IEnumerable<Course>> GetEnrolledCoursesByUser()
        {
            return await GetEnrolledCoursesByUserId();
        }

        // Lấy khóa học user đã đăng ký với phân trang (chỉ phân trang, không filter) - RLS đã filter theo userId
        public async Task<PagedResult<Course>> GetEnrolledCoursesByUserPagedAsync(PageRequest request)
        {
            var query = _context.UserCourses
                // RLS đã filter theo userId, không cần .Where(uc => uc.UserId == userId)
                .Include(uc => uc.Course)
                    .ThenInclude(c => c!.Teacher)
                .Include(uc => uc.Course)
                    .ThenInclude(c => c!.Lessons)
                .Select(uc => uc.Course!)
                .OrderBy(c => c.Title) // Sort theo Title mặc định
                .AsQueryable();

            return await query.ToPagedListAsync(request.PageNumber, request.PageSize);
        }


        // Kiểm tra user đã đăng ký khóa học chưa 

        public async Task<bool> IsUserEnrolled(int courseId, int userId)
        {
            return await IsUserEnrolledInCourse(userId, courseId);
        }

        // tìm kiem khoa học theo classcode 
        public async Task<IEnumerable<Course>> SearchCoursesByClassCode(string keyword)
        {
            return await _context.Courses
                .Where(c => c.ClassCode == keyword)
                .ToListAsync();

        }
        public async Task<IEnumerable<Course>> SearchCourses(string keyword)
        {
            return await _context.Courses
                .Where(c => EF.Functions.ILike(c.Title, $"%{keyword}%") ||
                            EF.Functions.ILike(c.DescriptionMarkdown, $"%{keyword}%"))
                .OrderBy(c => c.Title)
                .Take(10)
                .ToListAsync();
        }

        // Lấy thông tin enrollment của user trong course
        public async Task<UserCourse?> GetUserCourseAsync(int userId, int courseId)
        {
            return await _context.UserCourses
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == courseId);
        }

        // Lấy danh sách loại khóa học (System/Teacher) - từ Enum
        public async Task<IEnumerable<CourseTypeDto>> GetCourseTypesAsync()
        {
            // Trả về Task để tuân thủ async pattern, nhưng data từ Enum (không query DB)
            return await Task.FromResult(
                Enum.GetValues(typeof(CourseType))
                    .Cast<CourseType>()
                    .Select(ct => new CourseTypeDto
                    {
                        Value = (int)ct,
                        Name = ct.ToString(),
                        DisplayName = ct switch
                        {
                            CourseType.System => "Khóa học hệ thống",
                            CourseType.Teacher => "Khóa học giáo viên",
                            _ => ct.ToString()
                        }
                    })
            );
        }
    }
}
