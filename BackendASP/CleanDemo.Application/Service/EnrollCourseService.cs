using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Domain.Entities;
using CleanDemo.Domain.Enums;
using CleanDemo.Application.Common;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace CleanDemo.Application.Service
{
    public class EnrollCourseService : IEnrollCourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EnrollCourseService> _logger;

        public EnrollCourseService(
            ICourseRepository courseRepository,
            IPaymentRepository paymentRepository,
            IMapper mapper,
            ILogger<EnrollCourseService> logger)
        {
            _courseRepository = courseRepository;
            _paymentRepository = paymentRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// User - Đăng ký khóa học
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
        /// User - Hủy đăng ký khóa học
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

        /// <summary>
        /// User - Lấy danh sách khóa học đã đăng ký
        /// </summary>
        public async Task<ServiceResponse<IEnumerable<CourseResponseDto>>> GetMyEnrolledCoursesAsync(int userId)
        {
            var response = new ServiceResponse<IEnumerable<CourseResponseDto>>();

            try
            {
                var courses = await _courseRepository.GetEnrolledCoursesByUser(userId);
                var courseDtos = new List<CourseResponseDto>();

                foreach (var course in courses)
                {
                    var courseDto = _mapper.Map<CourseResponseDto>(course);
                    courseDto.LessonCount = await _courseRepository.CountLessons(course.CourseId);
                    courseDto.StudentCount = await _courseRepository.CountEnrolledUsers(course.CourseId);

                    courseDtos.Add(courseDto);
                }

                response.Data = courseDtos;
                response.Message = "Retrieved enrolled courses successfully";

                _logger.LogInformation("User {UserId} retrieved {Count} enrolled courses", userId, courseDtos.Count);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving enrolled courses: {ex.Message}";
                _logger.LogError(ex, "Error in GetMyEnrolledCoursesAsync for UserId: {UserId}", userId);
            }

            return response;
        }

        /// <summary>
        /// Teacher - Tham gia khóa học của teacher khác
        /// </summary>
        public async Task<ServiceResponse<bool>> JoinCourseAsTeacherAsync(JoinCourseTeacherDto joinDto, int teacherId)
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

                // Logic tham gia course (có thể implement sau)
                response.Success = true;
                response.Data = true;
                response.Message = "Successfully joined course as teacher";

                _logger.LogInformation("Teacher {TeacherId} joined course {CourseId}", teacherId, joinDto.CourseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error joining course: {ex.Message}";
                _logger.LogError(ex, "Error in JoinCourseAsTeacherAsync for TeacherId: {TeacherId}, CourseId: {CourseId}", teacherId, joinDto.CourseId);
            }

            return response;
        }
    }
}
