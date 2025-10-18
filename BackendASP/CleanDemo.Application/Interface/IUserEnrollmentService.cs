using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Interface
{
    public interface IUserEnrollmentService
    {
        /// <summary>
        /// User đăng ký khóa học thông thường (có thể miễn phí hoặc trả phí)
        /// </summary>
        Task<ServiceResponse<bool>> EnrollInCourseAsync(EnrollCourseDto enrollDto, int userId);

        /// <summary>
        /// User đăng ký khóa học do teacher tạo (có giới hạn học viên)
        /// </summary>
        Task<ServiceResponse<bool>> JoinTeacherCourseAsync(JoinCourseTeacherDto joinDto, int userId);

        /// <summary>
        /// User hủy đăng ký khóa học
        /// </summary>
        Task<ServiceResponse<bool>> UnenrollFromCourseAsync(int courseId, int userId);
    }
}
