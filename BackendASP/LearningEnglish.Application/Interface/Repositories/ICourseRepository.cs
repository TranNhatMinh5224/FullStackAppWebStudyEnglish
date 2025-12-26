using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Interface
{
    public interface ICourseRepository
    {
        // Lấy khóa học theo ID (bao gồm Teacher, Lessons, UserCourses)
        Task<Course?> GetCourseById(int courseId);
        
        // Thêm khóa học
        Task AddCourse(Course course);
        
        // Cập nhật khóa học
        Task UpdateCourse(Course course);
        
        // Xóa khóa học
        Task DeleteCourse(int courseId);
        
        // Lấy tất cả khóa học với phân trang (sử dụng AdminCourseQueryParameters) - cho Admin, sort theo Title
        Task<PagedResult<Course>> GetAllCoursesPagedForAdminAsync(AdminCourseQueryParameters parameters);
        
        // Lấy khóa học hệ thống
        Task<IEnumerable<Course>> GetSystemCourses();
        
        // Lấy khóa học của giáo viên - RLS đã filter theo teacherId
        Task<IEnumerable<Course>> GetCoursesByTeacher();

        // Lấy khóa học của giáo viên với phân trang (chỉ phân trang, không filter) - RLS đã filter theo teacherId
        Task<PagedResult<Course>> GetCoursesByTeacherPagedAsync(PageRequest request);

        // Lấy khóa học đã đăng ký của user - RLS đã filter theo userId
        Task<IEnumerable<Course>> GetEnrolledCoursesByUser();
        
        // Lấy khóa học đã đăng ký của user với phân trang (chỉ phân trang, không filter) - RLS đã filter theo userId
        Task<PagedResult<Course>> GetEnrolledCoursesByUserPagedAsync(PageRequest request);

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
        
        // Lấy danh sách loại khóa học (System/Teacher) - từ Enum
        // Dùng cho giao diện quản lý Admin: render dropdown filter để lọc danh sách khóa học
        Task<IEnumerable<CourseTypeDto>> GetCourseTypesAsync();
    }
}
