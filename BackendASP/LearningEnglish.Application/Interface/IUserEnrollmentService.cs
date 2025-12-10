using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IUserEnrollmentService
    {
        // User đăng ký khóa học (hỗ trợ cả course hệ thống và course teacher tạo)
        // - Course miễn phí (Price = 0 hoặc null): Đăng ký trực tiếp
        // - Course có phí (Price > 0): Kiểm tra thanh toán trước khi enroll (cả System và Teacher)
        // - Course Teacher: Kiểm tra thêm giới hạn Teacher Package
        Task<ServiceResponse<bool>> EnrollInCourseAsync(EnrollCourseDto enrollDto, int userId);

        // User hủy đăng ký khóa học
        Task<ServiceResponse<bool>> UnenrollFromCourseAsync(int courseId, int userId);


        // tham gia lớp học qua mã lớp học 
        Task<ServiceResponse<bool>> EnrollInCourseByClassCodeAsync(string classCode, int userId);
    }
}
