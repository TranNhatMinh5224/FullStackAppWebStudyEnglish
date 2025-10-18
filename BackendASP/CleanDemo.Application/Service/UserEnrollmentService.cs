using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Domain.Entities;
using CleanDemo.Domain.Enums;
using CleanDemo.Application.Common;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace CleanDemo.Application.Service
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
        /// User đăng ký khóa học thông thường (có thể miễn phí hoặc trả phí)
        /// </summary>
        public async Task<ServiceResponse<bool>> EnrollInCourseAsync(EnrollCourseDto enrollDto, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                if (enrollDto == null || enrollDto.CourseId <= 0)
                {
                    response.Success = false;
                    response.Message = "Invalid course ID";
                    return response;
                }

                // Kiểm tra course tồn tại
                var course = await _courseRepository.GetByIdAsync(enrollDto.CourseId);
                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found";
                    return response;
                }

                // Kiểm tra user đã đăng ký chưa
                if (await _courseRepository.IsUserEnrolled(enrollDto.CourseId, userId))
                {
                    response.Success = false;
                    response.Message = "User already enrolled in this course";
                    return response;
                }

                // Kiểm tra giá khóa học
                if (course.Price.HasValue && course.Price.Value > 0)
                {
                    // Tạo payment record (giả định thanh toán ngay lập tức thành công)
                    var payment = new Payment
                    {
                        UserId = userId,
                        CourseId = enrollDto.CourseId,
                        Amount = course.Price.Value,
                        Status = PaymentStatus.Completed  // Giả định thanh toán thành công
                    };

                    await _paymentRepository.AddPaymentAsync(payment);

                    _logger.LogInformation("Payment created for User {UserId} enrolling in Course {CourseId} with Amount {Amount}", userId, enrollDto.CourseId, course.Price.Value);
                }

                // Đăng ký user vào course
                await _courseRepository.EnrollUserInCourse(enrollDto.CourseId, userId);

                response.Success = true;
                response.Data = true;
                response.Message = "Successfully enrolled in course";

                _logger.LogInformation("User {UserId} enrolled in course {CourseId}", userId, enrollDto.CourseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error enrolling in course: {ex.Message}";
                _logger.LogError(ex, "Error in EnrollInCourseAsync for UserId: {UserId}, CourseId: {CourseId}", userId, enrollDto.CourseId);
            }

            return response;
        }

        /// <summary>
        /// User đăng ký khóa học do teacher tạo (có giới hạn học viên)
        /// </summary>
        public async Task<ServiceResponse<bool>> JoinTeacherCourseAsync(JoinCourseTeacherDto joinDto, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                if (joinDto == null || joinDto.CourseId <= 0)
                {
                    response.Success = false;
                    response.Message = "Invalid course ID";
                    return response;
                }

                // Kiểm tra course tồn tại
                var course = await _courseRepository.GetByIdAsync(joinDto.CourseId);
                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found";
                    return response;
                }

                // Kiểm tra course có phải Teacher course không
                if (course.Type != Domain.Enums.CourseType.Teacher)
                {
                    response.Success = false;
                    response.Message = "Can only join Teacher courses";
                    return response;
                }

                // Kiểm tra user đã tham gia chưa
                if (await _courseRepository.IsUserEnrolled(joinDto.CourseId, userId))
                {
                    response.Success = false;
                    response.Message = "You have already joined this course";
                    return response;
                }

                // Kiểm tra course có teacherId không
                if (!course.TeacherId.HasValue)
                {
                    response.Success = false;
                    response.Message = "Course does not have a teacher assigned";
                    return response;
                }

                // Lấy thông tin package của chủ khóa học
                var courseOwnerPackage = await _teacherPackageRepository.GetInformationTeacherpackage(course.TeacherId.Value);

                if (courseOwnerPackage == null)
                {
                    response.Success = false;
                    response.Message = "Course owner does not have an active subscription";
                    return response;
                }

                // Kiểm tra số lượng học viên hiện tại
                int currentStudentCount = await _courseRepository.CountEnrolledUsers(joinDto.CourseId);
                int maxStudents = courseOwnerPackage.MaxStudents;

                if (currentStudentCount >= maxStudents)
                {
                    response.Success = false;
                    response.Message = $"Course is full ({currentStudentCount}/{maxStudents} students)";
                    return response;
                }

                // Đăng ký user vào course
                await _courseRepository.EnrollUserInCourse(joinDto.CourseId, userId);

                response.Success = true;
                response.Data = true;
                response.Message = "Successfully joined course";

                _logger.LogInformation("User {UserId} joined teacher course {CourseId}", userId, joinDto.CourseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error joining course: {ex.Message}";
                _logger.LogError(ex, "Error in JoinTeacherCourseAsync for UserId: {UserId}, CourseId: {CourseId}", userId, joinDto.CourseId);
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
                if (courseId <= 0)
                {
                    response.Success = false;
                    response.Message = "Invalid course ID";
                    return response;
                }

                // Kiểm tra user đã đăng ký chưa
                if (!await _courseRepository.IsUserEnrolled(courseId, userId))
                {
                    response.Success = false;
                    response.Message = "User is not enrolled in this course";
                    return response;
                }

                // Hủy đăng ký
                await _courseRepository.UnenrollUserFromCourse(courseId, userId);

                response.Success = true;
                response.Data = true;
                response.Message = "Successfully unenrolled from course";

                _logger.LogInformation("User {UserId} unenrolled from course {CourseId}", userId, courseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error unenrolling from course: {ex.Message}";
                _logger.LogError(ex, "Error in UnenrollFromCourseAsync for UserId: {UserId}, CourseId: {CourseId}", userId, courseId);
            }

            return response;
        }
    }
}
