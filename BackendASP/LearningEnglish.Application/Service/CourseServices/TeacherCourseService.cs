using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Constants;
using LearningEnglish.Application.Common.Utils;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.Interface.Infrastructure.ImageService;
using AutoMapper;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    
    public class TeacherCourseService : ITeacherCourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherCourseService> _logger;
        private readonly ITeacherPackageRepository _teacherPackageRepository;
        private readonly ICourseImageService _courseImageService;

        public TeacherCourseService(
            ICourseRepository courseRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<TeacherCourseService> logger,
            ITeacherPackageRepository teacherPackageRepository,
            ICourseImageService courseImageService)
        {
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
            _teacherPackageRepository = teacherPackageRepository;
            _courseImageService = courseImageService;
        }
        // Tạo Khóa học 

        public async Task<ServiceResponse<CourseResponseDto>> CreateCourseAsync(
            TeacherCreateCourseRequestDto requestDto,
            int teacherId)
        {
            var response = new ServiceResponse<CourseResponseDto>();
            var classCode = ClassCodeGenerator.Generate();

            try
            {
                // Kiểm tra teacher tồn tại
                var teacher = await _userRepository.GetByIdAsync(teacherId);
                if (teacher == null)
                {
                    response.Success = false;
                    response.Message = "Teacher not found";
                    return response;
                }

                // Kiểm tra teacher có subscription active không
                var teacherPackage = await _teacherPackageRepository.GetInformationTeacherpackage(teacherId);
                if (teacherPackage == null)
                {
                    response.Success = false;
                    response.Message = "You need an active subscription to create courses";
                    return response;
                }

                // Kiểm tra số lượng course hiện tại của teacher này
                var teacherCourses = await _courseRepository.GetCoursesByTeacher(teacherId);
                int currentCourseCount = teacherCourses.Count();
                int maxCourses = teacherPackage.MaxCourses;

                if (currentCourseCount >= maxCourses)
                {
                    response.Success = false;
                    response.Message = $"You have reached the maximum number of courses ({currentCourseCount}/{maxCourses}). Please upgrade your package.";
                    return response;
                }

                // Kiểm tra MaxStudent của course không vượt quá giới hạn package
                if (requestDto.MaxStudent > 0 && requestDto.MaxStudent > teacherPackage.MaxStudents)
                {
                    response.Success = false;
                    response.Message = $"MaxStudent ({requestDto.MaxStudent}) cannot exceed your package limit ({teacherPackage.MaxStudents}). Please upgrade your package.";
                    return response;
                }

                // Nếu teacher không set MaxStudent (0), tự động set = MaxStudents của package
                int courseMaxStudent = requestDto.MaxStudent > 0 ? requestDto.MaxStudent : teacherPackage.MaxStudents;

                // Tạo course entity
                var course = new Course
                {
                    Title = requestDto.Title,
                    DescriptionMarkdown = requestDto.Description,
                    Type = requestDto.Type,
                    TeacherId = teacherId,
                    ClassCode = classCode,
                    MaxStudent = courseMaxStudent,
                    EnrollmentCount = 0,
                    IsFeatured = false
                };

                string? committedImageKey = null;

                // Convert temp file → real file nếu có ImageTempKey
                if (!string.IsNullOrWhiteSpace(requestDto.ImageTempKey))
                {
                    try
                    {
                        committedImageKey = await _courseImageService.CommitImageAsync(requestDto.ImageTempKey);
                        course.ImageKey = committedImageKey;
                        course.ImageType = requestDto.ImageType;
                    }
                    catch (Exception imageEx)
                    {
                        _logger.LogError(imageEx, "Failed to commit course image");
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu ảnh khóa học. Vui lòng thử lại.";
                        return response;
                    }
                }

                try
                {
                    await _courseRepository.AddCourse(course);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while creating course");

                    // Rollback image if DB fails
                    if (committedImageKey != null)
                    {
                        await _courseImageService.DeleteImageAsync(committedImageKey);
                    }

                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = "Lỗi database khi tạo khóa học";
                    return response;
                }

                // Map response và generate URL từ key
                var courseResponseDto = _mapper.Map<CourseResponseDto>(course);
                courseResponseDto.LessonCount = 0;
                courseResponseDto.StudentCount = 0;

                if (!string.IsNullOrWhiteSpace(course.ImageKey))
                {
                    courseResponseDto.ImageUrl = _courseImageService.BuildImageUrl(course.ImageKey);
                    courseResponseDto.ImageType = course.ImageType;
                }

                response.Success = true;
                response.Data = courseResponseDto;
                response.Message = $"Course created successfully ({currentCourseCount + 1}/{maxCourses} courses)";

                _logger.LogInformation(
                    "Teacher {TeacherId} created course: {CourseTitle} (ID: {CourseId})",
                    teacherId,
                    requestDto.Title,
                    course.CourseId
                );
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating course: {ex.Message}";
                _logger.LogError(ex, "Error in CreateCourseAsync for TeacherId: {TeacherId}", teacherId);
            }

            return response;
        }
        // Cập nhật khóa học

        public async Task<ServiceResponse<CourseResponseDto>> UpdateCourseAsync(
            int courseId,
            TeacherUpdateCourseRequestDto requestDto,
            int teacherId)
        {
            var response = new ServiceResponse<CourseResponseDto>();

            try
            {
                var course = await _courseRepository.GetCourseByIdForTeacher(courseId, teacherId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học hoặc bạn không có quyền truy cập";
                    return response;
                }

                // Không cho phép teacher cập nhật course System
                if (course.Type ==  CourseType.System)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Không thể cập nhật khóa học System";
                    return response;
                }

                // Kiểm tra package limit khi update MaxStudent
                var teacherPackage = await _teacherPackageRepository.GetInformationTeacherpackage(teacherId);
                if (teacherPackage == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy gói đăng ký đang hoạt động";
                    return response;
                }

                // Cập nhật Title nếu có
                if (!string.IsNullOrWhiteSpace(requestDto.Title))
                {
                    course.Title = requestDto.Title;
                }

                // Cập nhật Description nếu có
                if (!string.IsNullOrWhiteSpace(requestDto.Description))
                {
                    course.DescriptionMarkdown = requestDto.Description;
                }

                // Cập nhật MaxStudent nếu có
                if (requestDto.MaxStudent.HasValue && requestDto.MaxStudent.Value > 0)
                {
                    // Kiểm tra MaxStudent không vượt quá package limit
                    if (requestDto.MaxStudent.Value > teacherPackage.MaxStudents)
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = $"Số học sinh tối đa ({requestDto.MaxStudent.Value}) không được vượt quá giới hạn gói ({teacherPackage.MaxStudents})";
                        return response;
                    }

                    // Nếu đã có students enrolled, không cho phép giảm MaxStudent xuống dưới EnrollmentCount
                    if (requestDto.MaxStudent.Value < course.EnrollmentCount)
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = $"Không thể đặt số học sinh tối đa ({requestDto.MaxStudent.Value}) thấp hơn số lượng đã đăng ký ({course.EnrollmentCount})";
                        return response;
                    }

                    course.MaxStudent = requestDto.MaxStudent.Value;
                }

                string? newImageKey = null;
                string? oldImageKey = !string.IsNullOrWhiteSpace(course.ImageKey) ? course.ImageKey : null;

                // Xử lý file ảnh: commit new first
                if (!string.IsNullOrWhiteSpace(requestDto.ImageTempKey))
                {
                    try
                    {
                        newImageKey = await _courseImageService.CommitImageAsync(requestDto.ImageTempKey);
                        course.ImageKey = newImageKey;
                        course.ImageType = requestDto.ImageType;
                    }
                    catch (Exception imageEx)
                    {
                        _logger.LogError(imageEx, "Failed to commit new course image");
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể cập nhật ảnh khóa học.";
                        return response;
                    }
                }

                try
                {
                    await _courseRepository.UpdateCourse(course);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while updating course");

                    // Rollback new image if DB fails
                    if (newImageKey != null)
                    {
                        await _courseImageService.DeleteImageAsync(newImageKey);
                    }

                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = "Lỗi database khi cập nhật khóa học";
                    return response;
                }

                // Delete old image only after successful DB update
                if (oldImageKey != null && newImageKey != null)
                {
                    await _courseImageService.DeleteImageAsync(oldImageKey);
                }

                // Map response và generate URL từ key
                var courseResponseDto = _mapper.Map<CourseResponseDto>(course);
                courseResponseDto.LessonCount = await _courseRepository.CountLessons(courseId);
                courseResponseDto.StudentCount = await _courseRepository.CountEnrolledUsers(courseId);

                if (!string.IsNullOrWhiteSpace(course.ImageKey))
                {
                    courseResponseDto.ImageUrl = _courseImageService.BuildImageUrl(course.ImageKey);
                    courseResponseDto.ImageType = course.ImageType;
                }

                response.StatusCode = 200;
                response.Success = true;
                response.Data = courseResponseDto;
                response.Message = "Cập nhật khóa học thành công";

                _logger.LogInformation("Course {CourseId} updated by Teacher {TeacherId}", courseId, teacherId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi cập nhật khóa học";
                _logger.LogError(ex, "Error in UpdateCourseAsync for CourseId: {CourseId}", courseId);
            }

            return response;
        }

        // Lấy danh sách khóa học của teacher với phân trang
        public async Task<ServiceResponse<PagedResult<CourseResponseDto>>> GetMyCoursesPagedAsync(int teacherId, PageRequest request)
        {
            var response = new ServiceResponse<PagedResult<CourseResponseDto>>();
            try
            {
                var pagedData = await _courseRepository.GetCoursesByTeacherPagedAsync(teacherId, request);

                var items = new List<CourseResponseDto>();
                foreach (var course in pagedData.Items)
                {
                    var dto = _mapper.Map<CourseResponseDto>(course);
                    dto.LessonCount = await _courseRepository.CountLessons(course.CourseId);
                    dto.StudentCount = await _courseRepository.CountEnrolledUsers(course.CourseId);

                    if (!string.IsNullOrWhiteSpace(course.ImageKey))
                    {
                        dto.ImageUrl = _courseImageService.BuildImageUrl(course.ImageKey);
                        dto.ImageType = course.ImageType;
                    }
                    items.Add(dto);
                }

                response.Data = new PagedResult<CourseResponseDto>
                {
                    Items = items,
                    TotalCount = pagedData.TotalCount,
                    PageNumber = pagedData.PageNumber,
                    PageSize = pagedData.PageSize
                };
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
                _logger.LogError(ex, "Error in GetMyCoursesPagedAsync");
            }
            return response;
        }

      
        public async Task<ServiceResponse<CourseResponseDto>> DeleteCourseAsync(int courseId, int teacherId)
        {
            var response = new ServiceResponse<CourseResponseDto>();

            try
            {
                var course = await _courseRepository.GetCourseByIdForTeacher(courseId, teacherId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học hoặc bạn không có quyền truy cập";
                    return response;
                }

                // Không cho phép teacher xóa course System
                if (course.Type == CourseType.System)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Không thể xóa khóa học System";
                    return response;
                }

                // Xóa ảnh khóa học trên MinIO nếu có (trước khi xóa course)
                if (!string.IsNullOrWhiteSpace(course.ImageKey))
                {
                    await _courseImageService.DeleteImageAsync(course.ImageKey);
                }

                await _courseRepository.DeleteCourse(courseId);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Course deleted successfully";

                _logger.LogInformation("Course {CourseId} deleted", courseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "An error occurred while deleting the course";
                _logger.LogError(ex, "Error in DeleteCourseAsync for CourseId: {CourseId}", courseId);
            }

            return response;
        }
        

        // Lấy chi tiết 1 khóa học

        public async Task<ServiceResponse<TeacherCourseDetailDto>> GetCourseDetailAsync(int courseId, int teacherId)
        {
            var response = new ServiceResponse<TeacherCourseDetailDto>();

            try
            {
                var course = await _courseRepository.GetCourseById(courseId);

                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                // Kiểm tra course phải thuộc về teacher hiện tại
                if (course.TeacherId != teacherId)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền xem khóa học này";
                    return response;
                }

                // Không cho phép teacher xem course System (optional - tùy business logic)
                if (course.Type == CourseType.System)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Không thể xem chi tiết khóa học System";
                    return response;
                }

                // Map course entity to detailed DTO using AutoMapper
                var courseDetailDto = _mapper.Map<TeacherCourseDetailDto>(course);

                if (!string.IsNullOrWhiteSpace(course.ImageKey))
                {
                    courseDetailDto.ImageUrl = _courseImageService.BuildImageUrl(course.ImageKey);
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Data = courseDetailDto;
                response.Message = "Course details retrieved successfully";

                _logger.LogInformation("Retrieved details for Course {CourseId}", courseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "An error occurred while retrieving course details";
                _logger.LogError(ex, "Error in GetCourseDetailAsync for CourseId: {CourseId}", courseId);
            }

            return response;
        }
    }
}
