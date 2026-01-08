using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IUserEnrollmentService
    {
        // User đăng ký khóa học
        Task<ServiceResponse<bool>> EnrollInCourseAsync(EnrollCourseDto enrollDto, int userId);

        // User hủy đăng ký khóa học
        Task<ServiceResponse<bool>> UnenrollFromCourseAsync(int courseId, int userId);


        // tham gia lớp học qua mã lớp học 
        Task<ServiceResponse<bool>> EnrollInCourseByClassCodeAsync(string classCode, int userId);
    }
}
