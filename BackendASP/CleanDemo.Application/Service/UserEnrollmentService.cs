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
                    response.Message = "Course not found";
                    return response;
                }

                // Kiểm tra xem user đã đăng ký chưa
                if (await _courseRepository.IsUserEnrolled(enrollDto.CourseId, userId))
                {
                    response.Success = false;
                    response.Message = "User already enrolled in this course";
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
                        response.Message = "Hãy thanh toán khóa học trước khi đăng ký";
                        return response;
                    }
                }

                // Kiểm tra course có còn chỗ không (sử dụng business logic từ Entity)
                if (!course.CanJoin())
                {
                    response.Success = false;
                    response.Message = $"Course is full ({course.EnrollmentCount}/{course.MaxStudent}). Cannot enroll more students.";
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
                            response.Message = $"Teacher has reached maximum students limit ({totalStudents}/{teacherPackage.MaxStudents})";
                            return response;
                        }
                    }
                }

                // Đăng ký user vào course
                await _courseRepository.EnrollUserInCourse(userId, enrollDto.CourseId);

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
