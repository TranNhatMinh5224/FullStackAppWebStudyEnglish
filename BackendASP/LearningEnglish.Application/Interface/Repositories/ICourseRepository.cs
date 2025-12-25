using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Interface
{
    public interface ICourseRepository
    {
        // Lấy khóa học theo ID
        Task<Course?> GetCourseById(int courseId);
        Task<Course?> GetByIdAsync(int courseId);
        
        // Lấy khóa học với chi tiết
        Task<Course?> GetCourseWithDetails(int courseId);
        
        // Thêm khóa học
        Task AddCourse(Course course);
        
        // Cập nhật khóa học
        Task UpdateCourse(Course course);
        
        // Xóa khóa học
        Task DeleteCourse(int courseId);

        // Lấy tất cả khóa học
        Task<IEnumerable<Course>> GetAllCourses();
        
        // Lấy tất cả khóa học với phân trang (sử dụng CourseQueryParameters)
        Task<PagedResult<Course>> GetAllCoursesPagedAsync(CourseQueryParameters parameters);
        
        // Lấy khóa học hệ thống
        Task<IEnumerable<Course>> GetSystemCourses();
        
        // Lấy khóa học của giáo viên
        Task<IEnumerable<Course>> GetCoursesByTeacher(int teacherId);

        // Lấy khóa học của giáo viên với phân trang
        Task<PagedResult<Course>> GetCoursesByTeacherPagedAsync(int teacherId, CourseQueryParameters parameters);

        // Lấy khóa học đã đăng ký của user
        Task<IEnumerable<Course>> GetEnrolledCoursesByUser(int userId);
        
        // Lấy khóa học đã đăng ký của user với phân trang
        Task<PagedResult<Course>> GetEnrolledCoursesByUserPagedAsync(int userId, EnrolledCourseQueryParameters parameters);

        // Kiểm tra user đã đăng ký khóa học
        Task<bool> IsUserEnrolled(int courseId, int userId);
        
        // Lấy thông tin đăng ký khóa học
        Task<UserCourse?> GetUserCourseAsync(int userId, int courseId);
        
        // Đăng ký khóa học
        Task EnrollUserInCourse(int userId, int courseId);
        
        // Hủy đăng ký khóa học
        Task UnenrollUserFromCourse(int courseId, int userId);

        // Đếm số bài học
        Task<int> CountLessons(int courseId);
        
        // Đếm số học viên đã đăng ký
        Task<int> CountEnrolledUsers(int courseId);
        
        // Lấy danh sách học viên đã đăng ký
        Task<IEnumerable<User>> GetEnrolledUsers(int courseId);
        
        // Lấy tổng số học viên của giáo viên
        Task<int> GetTotalStudentsByTeacher(int teacherId);

        // Tìm kiếm khóa học theo mã lớp
        Task<IEnumerable<Course>> SearchCoursesByClassCode(string keyword);
        
        // Tìm kiếm khóa học
        Task<IEnumerable<Course>> SearchCourses(string keyword);
        
        // Statistics methods cho Admin Dashboard
        Task<int> GetTotalCoursesCountAsync();
        Task<int> GetCourseCountByTypeAsync(CourseType type);
        Task<int> GetCourseCountByStatusAsync(CourseStatus status);
        Task<int> GetTotalEnrollmentsCountAsync();
        Task<int> GetNewCoursesCountAsync(DateTime fromDate);
        
        // Teacher statistics methods
        Task<int> GetCoursesCountByTeachersAsync();
        Task<int> GetPublishedCoursesCountByTeachersAsync();
        Task<int> GetEnrollmentsCountForTeacherCoursesAsync();
        
        // Student statistics methods
        Task<int> GetStudentsWithEnrollmentsCountAsync();
        Task<int> GetActiveStudentsInCoursesCountAsync();
    }
}
