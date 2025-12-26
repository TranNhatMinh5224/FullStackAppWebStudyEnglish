using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Utils;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.Common.Pagination;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Application.Service
{
    public class TeacherCourseService : ITeacherCourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherCourseService> _logger;
        private readonly ITeacherPackageRepository _teacherPackageRepository;
        private readonly IMinioFileStorage _minioFileStorage;

        // Đặt bucket + folder cho ảnh khóa học 
        private const string CourseImageBucket = "courses";   // vd: bucket "images"
        private const string CourseImageFolder = "real";  // folder real "courses"

        public TeacherCourseService(
            ICourseRepository courseRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<TeacherCourseService> logger,
            ITeacherPackageRepository teacherPackageRepository,
            IMinioFileStorage minioFileStorage)
        {
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
            _teacherPackageRepository = teacherPackageRepository;
            _minioFileStorage = minioFileStorage;
        }

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

                // Kiểm tra số lượng course hiện tại - RLS đã filter theo teacherId
                var teacherCourses = await _courseRepository.GetCoursesByTeacher();
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
                    var commitResult = await _minioFileStorage.CommitFileAsync(
                        requestDto.ImageTempKey,
                        CourseImageBucket,
                        CourseImageFolder
                    );

                    if (!commitResult.Success || string.IsNullOrWhiteSpace(commitResult.Data))
                    {
                        response.Success = false;
                        response.Message = "Không thể lưu ảnh khóa học. Vui lòng thử lại.";
                        return response;
                    }

                    committedImageKey = commitResult.Data;
                    course.ImageKey = committedImageKey;
                    course.ImageType = requestDto.ImageType;
                }

                try
                {
                    await _courseRepository.AddCourse(course);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while creating course");

                    // Rollback MinIO file
                    if (committedImageKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(committedImageKey, CourseImageBucket);
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
                    courseResponseDto.ImageUrl = BuildPublicUrl.BuildURL(
                        CourseImageBucket,
                        course.ImageKey
                    );
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
                // RLS đã tự động filter courses theo TeacherId
                // Nếu course == null → course không tồn tại hoặc không thuộc teacher hiện tại
                // Trả 404 để không leak thông tin về sự tồn tại của course
                var course = await _courseRepository.GetCourseById(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
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

                // Kiểm tra MaxStudent không vượt quá package limit
                if (requestDto.MaxStudent > 0 && requestDto.MaxStudent > teacherPackage.MaxStudents)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Số học sinh tối đa ({requestDto.MaxStudent}) không được vượt quá giới hạn gói ({teacherPackage.MaxStudents})";
                    return response;
                }

                // Nếu đã có students enrolled, không cho phép giảm MaxStudent xuống dưới EnrollmentCount
                if (requestDto.MaxStudent > 0 && requestDto.MaxStudent < course.EnrollmentCount)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Không thể đặt số học sinh tối đa ({requestDto.MaxStudent}) thấp hơn số lượng đã đăng ký ({course.EnrollmentCount})";
                    return response;
                }

                // Cập nhật course basic info
                course.Title = requestDto.Title;
                course.DescriptionMarkdown = requestDto.Description;
                course.Type = requestDto.Type;
                course.MaxStudent = requestDto.MaxStudent > 0 ? requestDto.MaxStudent : teacherPackage.MaxStudents;

                string? newImageKey = null;
                string? oldImageKey = !string.IsNullOrWhiteSpace(course.ImageKey) ? course.ImageKey : null;

                // Xử lý file ảnh: commit new first
                if (!string.IsNullOrWhiteSpace(requestDto.ImageTempKey))
                {
                    // Commit ảnh mới
                    var commitResult = await _minioFileStorage.CommitFileAsync(
                        requestDto.ImageTempKey,
                        CourseImageBucket,
                        CourseImageFolder
                    );

                    if (!commitResult.Success || string.IsNullOrWhiteSpace(commitResult.Data))
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể cập nhật ảnh khóa học.";
                        return response;
                    }

                    newImageKey = commitResult.Data;
                    course.ImageKey = newImageKey;
                    course.ImageType = requestDto.ImageType;
                }

                try
                {
                    await _courseRepository.UpdateCourse(course);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while updating course");

                    // Rollback new image
                    if (newImageKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(newImageKey, CourseImageBucket);
                    }

                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = "Lỗi database khi cập nhật khóa học";
                    return response;
                }

                // Delete old image only after successful DB update
                if (oldImageKey != null && newImageKey != null)
                {
                    try
                    {
                        await _minioFileStorage.DeleteFileAsync(oldImageKey, CourseImageBucket);
                    }
                    catch
                    {
                        _logger.LogWarning("Failed to delete old course image: {ImageUrl}", oldImageKey);
                    }
                }

                // Map response và generate URL từ key
                var courseResponseDto = _mapper.Map<CourseResponseDto>(course);
                courseResponseDto.LessonCount = await _courseRepository.CountLessons(courseId);
                courseResponseDto.StudentCount = await _courseRepository.CountEnrolledUsers(courseId);

                if (!string.IsNullOrWhiteSpace(course.ImageKey))
                {
                    courseResponseDto.ImageUrl = BuildPublicUrl.BuildURL(
                        CourseImageBucket,
                        course.ImageKey
                    );
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

        // Lấy danh sách khóa học của teacher với phân trang (chỉ phân trang, không filter) - RLS đã filter
        public async Task<ServiceResponse<PagedResult<CourseResponseDto>>> GetMyCoursesPagedAsync(PageRequest request)
        {
            var response = new ServiceResponse<PagedResult<CourseResponseDto>>();
            try
            {
                var pagedData = await _courseRepository.GetCoursesByTeacherPagedAsync(request);

                var items = new List<CourseResponseDto>();
                foreach (var course in pagedData.Items)
                {
                    var dto = _mapper.Map<CourseResponseDto>(course);
                    dto.LessonCount = await _courseRepository.CountLessons(course.CourseId);
                    dto.StudentCount = await _courseRepository.CountEnrolledUsers(course.CourseId);

                    if (!string.IsNullOrWhiteSpace(course.ImageKey))
                    {
                        dto.ImageUrl = BuildPublicUrl.BuildURL(CourseImageBucket, course.ImageKey);
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

        // Xóa khóa học - RLS đã filter, chỉ check null → 404
        public async Task<ServiceResponse<CourseResponseDto>> DeleteCourseAsync(int courseId)
        {
            var response = new ServiceResponse<CourseResponseDto>();

            try
            {
                // RLS đã tự động filter courses theo TeacherId
                // Nếu course == null → course không tồn tại hoặc không thuộc teacher hiện tại
                // Trả 404 để không leak thông tin về sự tồn tại của course
                var course = await _courseRepository.GetCourseById(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Course not found";
                    return response;
                }

                await _courseRepository.DeleteCourse(courseId);
                // xóa ảnh khóa học trên MinIO nếu có
                if (!string.IsNullOrWhiteSpace(course.ImageKey))
                {
                    try
                    {
                        await _minioFileStorage.DeleteFileAsync(
                            course.ImageKey,
                            CourseImageBucket
                        );
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Failed to delete course image: {ImageUrl}", course.ImageKey);
                    }
                }

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

        public async Task<ServiceResponse<TeacherCourseDetailDto>> GetCourseDetailAsync(int courseId)
        {
            var response = new ServiceResponse<TeacherCourseDetailDto>();

            try
            {
                // RLS đã tự động filter courses theo TeacherId
                // Nếu course == null → course không tồn tại hoặc không thuộc teacher hiện tại
                // Trả 404 để không leak thông tin về sự tồn tại của course
                var course = await _courseRepository.GetCourseById(courseId);

                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Course not found";
                    return response;
                }

                // Map course entity to detailed DTO using AutoMapper
                var courseDetailDto = _mapper.Map<TeacherCourseDetailDto>(course);

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
