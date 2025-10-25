using CleanDemo.Domain.Entities;

namespace CleanDemo.Application.Interface
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
        Task<IEnumerable<Course>> GetAllCourses(); // Admin xem tất cả
        Task<IEnumerable<Course>> GetSystemCourses(); // User xem khóa học hệ thống
        Task<IEnumerable<Course>> GetCoursesByTeacher(int teacherId); // Khóa học của teacher
        Task<IEnumerable<Course>> GetEnrolledCoursesByUser(int userId); // Khóa học user đã đăng ký

        // === KIỂM TRA & ĐĂNG KÝ ===
        Task<bool> IsUserEnrolled(int courseId, int userId);
        Task EnrollUserInCourse(int courseId, int userId);
        Task UnenrollUserFromCourse(int courseId, int userId);

        // === THỐNG KÊ ===
        Task<int> CountLessons(int courseId);
        Task<int> CountEnrolledUsers(int courseId);
        Task<IEnumerable<User>> GetEnrolledUsers(int courseId);
        Task<int> GetTotalStudentsByTeacher(int teacherId); // THÊM: Tổng students của teacher
    }
}