using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Common;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class UserEnrollmentService : IUserEnrollmentService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITeacherPackageRepository _teacherPackageRepository;
        private readonly ILogger<UserEnrollmentService> _logger;

        public UserEnrollmentService(
            ICourseRepository courseRepository,
            IPaymentRepository paymentRepository,
            ITeacherPackageRepository teacherPackageRepository,
            ILogger<UserEnrollmentService> logger)
        {
            _courseRepository = courseRepository;
            _paymentRepository = paymentRepository;
            _teacherPackageRepository = teacherPackageRepository;
            _logger = logger;
        }

        /// <summary>
        /// User đăng ký khóa học (hỗ trợ cả course hệ thống và course teacher tạo)
        /// - Course miễn phí (Price = 0 hoặc null): Đăng ký trực tiếp
        /// - Course có phí (Price > 0): Kiểm tra thanh toán trước khi enroll
        /// - Course Teacher: Kiểm tra thêm giới hạn Teacher Package
        /// </summary>
        public async Task<ServiceResponse<bool>> EnrollInCourseAsync(EnrollCourseDto enrollDto, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Kiểm tra course tồn tại
                var course = await _courseRepository.GetByIdAsync(enrollDto.CourseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                // Kiểm tra xem user đã đăng ký chưa
                if (await _courseRepository.IsUserEnrolled(enrollDto.CourseId, userId))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Bạn đã đăng ký khóa học này rồi";
                    return response;
                }

                // Kiểm tra thanh toán cho TẤT CẢ course có phí (cả System và Teacher)
                // Chỉ course MIỄN PHÍ (Price = 0 hoặc null) mới skip payment check
                if (course.Price > 0)
                {
                    var payment = await _paymentRepository.GetSuccessfulPaymentByUserAndCourseAsync(userId, enrollDto.CourseId);
                    if (payment == null)
                    {
                        response.Success = false;
                        response.StatusCode = 402;
                        response.Message = "Hãy thanh toán khóa học trước khi đăng ký";
                        return response;
                    }
                }

                // Kiểm tra course có còn chỗ không (sử dụng business logic từ Entity)
                if (!course.CanJoin())
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Khóa học đã đầy ({course.EnrollmentCount}/{course.MaxStudent}). Không thể đăng ký thêm";
                    return response;
                }

                // Nếu là course của teacher, kiểm tra thêm giới hạn MaxStudents của package
                if (course.Type == CourseType.Teacher && course.TeacherId.HasValue)
                {
                    var teacherPackage = await _teacherPackageRepository.GetInformationTeacherpackage(course.TeacherId.Value);
                    if (teacherPackage != null)
                    {
                        // Đếm tổng số student hiện tại của teacher (tối ưu hơn)
                        int totalStudents = await _courseRepository.GetTotalStudentsByTeacher(course.TeacherId.Value);

                        if (totalStudents >= teacherPackage.MaxStudents)
                        {
                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = $"Giáo viên đã đạt giới hạn số học sinh ({totalStudents}/{teacherPackage.MaxStudents})";
                            return response;
                        }
                    }
                }

                // Đăng ký user vào course
                await _courseRepository.EnrollUserInCourse(userId, enrollDto.CourseId);

                response.Success = true;
                response.StatusCode = 200;
                response.Data = true;
                response.Message = "Đăng ký khóa học thành công";

                _logger.LogInformation("User {UserId} enrolled in course {CourseId}", userId, enrollDto.CourseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi đăng ký khóa học: {ex.Message}";
                _logger.LogError(ex, "Error in EnrollInCourseAsync for UserId: {UserId}, CourseId: {CourseId}", userId, enrollDto.CourseId);
            }

            return response;
        }

        /// <summary>
        /// User hủy đăng ký khóa học
        /// </summary>
        public async Task<ServiceResponse<bool>> UnenrollFromCourseAsync(int courseId, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Kiểm tra user đã đăng ký chưa
                if (!await _courseRepository.IsUserEnrolled(courseId, userId))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Bạn chưa đăng ký khóa học này";
                    return response;
                }

                // Hủy đăng ký
                await _courseRepository.UnenrollUserFromCourse(courseId, userId);

                response.Success = true;
                response.StatusCode = 200;
                response.Data = true;
                response.Message = "Hủy đăng ký khóa học thành công";

                _logger.LogInformation("User {UserId} unenrolled from course {CourseId}", userId, courseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi hủy đăng ký khóa học: {ex.Message}";
                _logger.LogError(ex, "Error in UnenrollFromCourseAsync for UserId: {UserId}, CourseId: {CourseId}", userId, courseId);
            }

            return response;
        }
        // tham gia lớp học qua mã lớp học
        public async Task<ServiceResponse<bool>> EnrollInCourseByClassCodeAsync(string classCode, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Tìm course theo classCode
                var courses = await _courseRepository.SearchCourses(classCode);
                var course = courses.FirstOrDefault();
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học với mã lớp học này";
                    return response;
                }

                // Kiểm tra xem user đã đăng ký chưa
                if (await _courseRepository.IsUserEnrolled(course.CourseId, userId))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Bạn đã đăng ký khóa học này rồi";
                    return response;
                }

                // Kiểm tra course có còn chỗ không
                if (!course.CanJoin())
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Khóa học đã đầy ({course.EnrollmentCount}/{course.MaxStudent}). Không thể đăng ký thêm";
                    return response;
                }

                // Đăng ký user vào course
                await _courseRepository.EnrollUserInCourse(userId, course.CourseId);

                response.Success = true;
                response.StatusCode = 200;
                response.Data = true;
                response.Message = "Đăng ký khóa học thành công qua mã lớp học";

                _logger.LogInformation("User {UserId} enrolled in course {CourseId} via class code", userId, course.CourseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi đăng ký khóa học qua mã lớp: {ex.Message}";
                _logger.LogError(ex, "Error in EnrollInCourseByClassCodeAsync for UserId: {UserId}, ClassCode: {ClassCode}", userId, classCode);
            }

            return response;
        }
    }
}
