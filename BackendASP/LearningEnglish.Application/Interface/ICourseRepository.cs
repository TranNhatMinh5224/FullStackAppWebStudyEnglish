using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface ICourseRepository
    {
        // === CRUD CƠ BẢN ===
        Task<Course?> GetCourseById(int courseId);
        Task<Course?> GetByIdAsync(int courseId);
        Task<Course?> GetCourseWithDetails(int courseId);
        Task AddCourse(Course course);
        Task UpdateCourse(Course course);
        Task DeleteCourse(int courseId);

        // === LẤY DANH SÁCH KHÓA HỌC ===
        Task<IEnumerable<Course>> GetAllCourses();
        
        // Lấy tất cả khóa học với phân trang
        Task<PagedResult<Course>> GetAllCoursesPagedAsync(PageRequest request);
        
        Task<IEnumerable<Course>> GetSystemCourses();
        Task<IEnumerable<Course>> GetCoursesByTeacher(int teacherId);

        // Lấy khóa học của giáo viên với phân trang
        Task<PagedResult<Course>> GetCoursesByTeacherPagedAsync(int teacherId, PageRequest request);

        Task<IEnumerable<Course>> GetEnrolledCoursesByUser(int userId);

        // === KIỂM TRA & ĐĂNG KÝ ===
        Task<bool> IsUserEnrolled(int courseId, int userId);
        Task<UserCourse?> GetUserCourseAsync(int userId, int courseId); // Lấy thông tin enrollment
        Task EnrollUserInCourse(int courseId, int userId);
        Task UnenrollUserFromCourse(int courseId, int userId);

        // === THỐNG KÊ ===
        Task<int> CountLessons(int courseId);
        Task<int> CountEnrolledUsers(int courseId);
        Task<IEnumerable<User>> GetEnrolledUsers(int courseId);
        Task<int> GetTotalStudentsByTeacher(int teacherId);

        Task<IEnumerable<Course>> SearchCoursesByClassCode(string keyword);
        Task<IEnumerable<Course>> SearchCourses(string keyword);
    }
}
