using CleanDemo.Domain.Entities;

namespace CleanDemo.Application.Interface
{
    public interface ICourseRepository
    {
        // === CRUD CƠ BẢN ===
        Task<Course?> GetCourseById(int courseId);
        Task<Course?> GetCourseWithDetails(int courseId);
        Task AddCourse(Course course);
        Task UpdateCourse(Course course);
        Task DeleteCourse(int courseId);

        // === LẤY DANH SÁCH KHÓA HỌC ===
        Task<IEnumerable<Course>> GetAllCourses(); // Admin xem tất cả
        Task<IEnumerable<Course>> GetAllCourseSystem(); // User xem khóa học hệ thống
        Task<IEnumerable<Course>> GetAllCoursesByTeacherId(int teacherId); // Khóa học của teacher
        Task<IEnumerable<Course>> GetEnrolledCoursesByUserId(int userId); // Khóa học user đã đăng ký
        Task<IEnumerable<Course>> GetEnrolledTeacherCoursesByUserId(int userId); // Khóa học teacher user đã tham gia

        // === KIỂM TRA & ĐĂNG KÝ ===
        Task<bool> IsUserEnrolledInCourse(int userId, int courseId);
        Task EnrollUserInCourse(int userId, int courseId);
        Task UnenrollUserFromCourse(int userId, int courseId);

        // === THỐNG KÊ ===
        Task<int> CountLessons(int courseId);
        Task<int> CountEnrolledUsers(int courseId);
        Task<IEnumerable<User>> GetEnrolledUsers(int courseId);
    }
}