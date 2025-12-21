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
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<UserEnrollmentService> _logger;

        public UserEnrollmentService(
            ICourseRepository courseRepository,
            IPaymentRepository paymentRepository,
            ITeacherPackageRepository teacherPackageRepository,
            INotificationRepository notificationRepository,
            ILogger<UserEnrollmentService> logger)
        {
            _courseRepository = courseRepository;
            _paymentRepository = paymentRepository;
            _teacherPackageRepository = teacherPackageRepository;
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        private async Task CreateEnrollmentNotificationAsync(int userId, string courseTitle)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = "🎉 Đăng ký khóa học thành công",
                    Message = $"Bạn đã đăng ký thành công khóa học '{courseTitle}'. Hãy bắt đầu học ngay!",
                    Type = NotificationType.CourseEnrollment,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.AddAsync(notification);
                _logger.LogInformation("Created enrollment notification for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create enrollment notification for user {UserId}", userId);
            }
        }

        // User đăng ký khóa học (hỗ trợ cả course hệ thống và course teacher tạo)
        // - Course miễn phí (Price = 0 hoặc null): Đăng ký trực tiếp
        // - Course có phí (Price > 0): Kiểm tra thanh toán trước khi enroll
        // - Course Teacher: Kiểm tra thêm giới hạn Teacher Package
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

                // Nếu là course của teacher, kiểm tra package status và giới hạn
                if (course.Type == CourseType.Teacher && course.TeacherId.HasValue)
                {
                    var teacherPackage = await _teacherPackageRepository.GetInformationTeacherpackage(course.TeacherId.Value);

                    // Check 1: Teacher phải có active package mới nhận students mới
                    if (teacherPackage == null)
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Khóa học này hiện không nhận học viên mới. Giáo viên cần gia hạn gói để tiếp tục nhận học viên.";
                        _logger.LogWarning("Teacher {TeacherId} has no active package. Cannot enroll new students in course {CourseId}",
                            course.TeacherId.Value, enrollDto.CourseId);
                        return response;
                    }
                }

                // Đăng ký user vào course 
                await _courseRepository.EnrollUserInCourse(userId, enrollDto.CourseId);

                // Tạo notification cho user
                await CreateEnrollmentNotificationAsync(userId, course.Title);

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

        // User hủy đăng ký khóa học
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
        // Tham gia lớp học qua mã lớp học (class code)
        // Lưu ý: Đăng ký qua class code thường là cho course MIỄN PHÍ 
        // Nếu course có phí, cần xử lý payment riêng
        public async Task<ServiceResponse<bool>> EnrollInCourseByClassCodeAsync(string classCode, int userId)
        {
            var response = new ServiceResponse<bool>();
            int? courseId = null;

            try
            {
                // Tìm course theo classCode
                var courses = await _courseRepository.SearchCoursesByClassCode(classCode);
                var course = courses.FirstOrDefault();
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học với mã lớp học này";
                    return response;
                }

                courseId = course.CourseId;

                // Kiểm tra xem user đã đăng ký chưa
                if (await _courseRepository.IsUserEnrolled(course.CourseId, userId))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Bạn đã đăng ký khóa học này rồi";
                    return response;
                }

                // Kiểm tra course có còn chỗ không (trước khi gọi EnrollUserInCourse)
                if (!course.CanJoin())
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Khóa học đã đầy ({course.EnrollmentCount}/{course.MaxStudent}). Không thể đăng ký thêm";
                    return response;
                }


                if (course.Type == CourseType.Teacher && course.TeacherId.HasValue)
                {
                    var teacherPackage = await _teacherPackageRepository.GetInformationTeacherpackage(course.TeacherId.Value);

                    if (teacherPackage == null)
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Khóa học này hiện không nhận học viên mới. Giáo viên cần gia hạn gói để tiếp tục nhận học viên.";
                        _logger.LogWarning("Teacher {TeacherId} has no active package. Cannot enroll student {UserId} via class code in course {CourseId}",
                            course.TeacherId.Value, userId, course.CourseId);
                        return response;
                    }
                }

                // Đăng ký user vào course
                await _courseRepository.EnrollUserInCourse(userId, course.CourseId);

                response.Success = true;
                response.StatusCode = 200;
                response.Data = true;
                response.Message = "Đăng ký khóa học thành công qua mã lớp học";

                _logger.LogInformation("User {UserId} enrolled in course {CourseId} via class code {ClassCode}", userId, course.CourseId, classCode);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("maximum capacity reached"))
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = "Bạn không thể tham gia vào lớp này vì khóa học đã đầy học viên";
                _logger.LogWarning("User {UserId} cannot enroll in course {CourseId} - class is full", userId, courseId);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already enrolled"))
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = "Bạn đã đăng ký khóa học này rồi";
                _logger.LogWarning("User {UserId} already enrolled in course {CourseId}", userId, courseId);
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
