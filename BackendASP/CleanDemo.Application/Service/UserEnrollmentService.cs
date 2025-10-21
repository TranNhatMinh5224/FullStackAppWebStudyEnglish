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
                // Kiểm tra course tồn tại
                var course = await _courseRepository.GetByIdAsync(enrollDto.CourseId);
                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found";
                    return response;
                }
                // Kiểm tra xem user đã đăng ký chưa
                if (await _courseRepository.IsUserEnrolled(enrollDto.CourseId, userId))
                {
                    if (course.Price > 0)
                    {
                        // Kiểm tra xem đã thanh toán chưa
                        var payment = await _paymentRepository.GetSuccessfulPaymentByUserAndCourseAsync(userId, enrollDto.CourseId);
                        if (payment == null)
                        {
                            response.Success = false;
                            response.Message = "hãy thanh toán khóa học trước khi đăng ký";
                            return response;
                        }
                    }
                    else
                    {
                        response.Success = false;
                        response.Message = "User already enrolled in this course";
                    }
                    return response;
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


                // Kiểm tra course tồn tại
                var course = await _courseRepository.GetByIdAsync(joinDto.CourseId);
                // kiểm tra xem user đã đăng ký chưa 

                // Kiểm tra xem user đã đăng ký chưa
                if (await _courseRepository.IsUserEnrolled(joinDto.CourseId, userId))
                {
                    response.Success = false;
                    response.Message = "User already enrolled in this course";
                    return response;
                }



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
