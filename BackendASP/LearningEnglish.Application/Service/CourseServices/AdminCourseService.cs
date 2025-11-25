using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class AdminCourseService : IAdminCourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminCourseService> _logger;
        private readonly IMinioFileStorage _minioFileStorage;

        // Đặt bucket + folder cho ảnh khóa học
        private const string CourseImageBucket = "courses";
        private const string CourseImageFolder = "real";

        public AdminCourseService(
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<AdminCourseService> logger,
            IMinioFileStorage minioFileStorage)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
            _minioFileStorage = minioFileStorage;
        }

        public async Task<ServiceResponse<IEnumerable<AdminCourseListResponseDto>>> GetAllCoursesAsync()
        {
            var response = new ServiceResponse<IEnumerable<AdminCourseListResponseDto>>();

            try
            {
                var courses = await _courseRepository.GetAllCourses();
                var courseDtos = new List<AdminCourseListResponseDto>();

                foreach (var course in courses)
                {
                    var courseDto = _mapper.Map<AdminCourseListResponseDto>(course);

                    // Thêm thống kê
                    courseDto.LessonCount = await _courseRepository.CountLessons(course.CourseId);
                    courseDto.StudentCount = await _courseRepository.CountEnrolledUsers(course.CourseId);
                    courseDto.TeacherName = course.Teacher?.FirstName + " " + course.Teacher?.LastName ?? "System Admin";

                    // Generate URL từ key
                    if (!string.IsNullOrWhiteSpace(course.ImageUrl))
                    {
                        courseDto.ImageUrl = BuildPublicUrl.BuildURL(
                            CourseImageBucket,
                            course.ImageUrl
                        );
                        courseDto.ImageType = course.ImageType;
                    }

                    courseDtos.Add(courseDto);
                }

                response.StatusCode = 200;
                response.Data = courseDtos;
                response.Message = "Lấy danh sách khóa học thành công";

                _logger.LogInformation("Admin retrieved {Count} courses", courseDtos.Count);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                _logger.LogError(ex, "Error in GetAllCoursesAsync");
            }

            return response;
        }

        public async Task<ServiceResponse<CourseResponseDto>> AdminCreateCourseAsync(AdminCreateCourseRequestDto requestDto)
        {
            var response = new ServiceResponse<CourseResponseDto>();

            try
            {
                // Tạo course entity
                var course = new Course
                {
                    Title = requestDto.Title,
                    Description = requestDto.Description,
                    Type = requestDto.Type,
                    Price = requestDto.Price,
                    TeacherId = null,
                    MaxStudent = requestDto.MaxStudent,
                    IsFeatured = requestDto.IsFeatured,
                    EnrollmentCount = 0
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
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu ảnh khóa học. Vui lòng thử lại.";
                        return response;
                    }

                    committedImageKey = commitResult.Data;
                    course.ImageUrl = committedImageKey;
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

                var courseResponseDto = _mapper.Map<CourseResponseDto>(course);
                courseResponseDto.TeacherName = "System Admin";
                courseResponseDto.LessonCount = 0;
                courseResponseDto.StudentCount = 0;

                // Generate URL từ key
                if (!string.IsNullOrWhiteSpace(course.ImageUrl))
                {
                    courseResponseDto.ImageUrl = BuildPublicUrl.BuildURL(
                        CourseImageBucket,
                        course.ImageUrl
                    );
                    courseResponseDto.ImageType = course.ImageType;
                }

                response.StatusCode = 201;
                response.Data = courseResponseDto;
                response.Message = "Tạo khóa học thành công";

                _logger.LogInformation("Admin created course: {CourseTitle} (ID: {CourseId})", requestDto.Title, course.CourseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                _logger.LogError(ex, "Error in AdminCreateCourseAsync");
            }

            return response;
        }

        public async Task<ServiceResponse<CourseResponseDto>> AdminUpdateCourseAsync(int courseId, AdminUpdateCourseRequestDto requestDto)
        {
            var response = new ServiceResponse<CourseResponseDto>();

            try
            {
                var course = await _courseRepository.GetByIdAsync(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                // Cập nhật course
                course.Title = requestDto.Title;
                course.Description = requestDto.Description;
                course.Price = requestDto.Price;
                course.MaxStudent = requestDto.MaxStudent;
                course.IsFeatured = requestDto.IsFeatured;
                course.Type = requestDto.Type;

                string? newImageKey = null;
                string? oldImageKey = !string.IsNullOrWhiteSpace(course.ImageUrl) ? course.ImageUrl : null;
                
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
                    course.ImageUrl = newImageKey;
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

                var courseResponseDto = _mapper.Map<CourseResponseDto>(course);
                courseResponseDto.LessonCount = await _courseRepository.CountLessons(courseId);
                courseResponseDto.StudentCount = await _courseRepository.CountEnrolledUsers(courseId);
                courseResponseDto.TeacherName = course.Teacher != null
                    ? $"{course.Teacher.FirstName} {course.Teacher.LastName}"
                    : "System Admin";

                // Generate URL từ key
                if (!string.IsNullOrWhiteSpace(course.ImageUrl))
                {
                    courseResponseDto.ImageUrl = BuildPublicUrl.BuildURL(
                        CourseImageBucket,
                        course.ImageUrl
                    );
                    courseResponseDto.ImageType = course.ImageType;
                }


                response.StatusCode = 200;
                response.Data = courseResponseDto;
                response.Message = "Cập nhật khóa học thành công";

                _logger.LogInformation("Admin updated course {CourseId}", courseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                _logger.LogError(ex, "Error in AdminUpdateCourseAsync for CourseId: {CourseId}", courseId);
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteCourseAsync(int courseId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var course = await _courseRepository.GetByIdAsync(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                // Xóa ảnh khóa học trên MinIO nếu có
                if (!string.IsNullOrWhiteSpace(course.ImageUrl))
                {
                    try
                    {
                        await _minioFileStorage.DeleteFileAsync(
                            course.ImageUrl,
                            CourseImageBucket
                        );
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Failed to delete course image: {ImageUrl}", course.ImageUrl);
                    }
                }

                await _courseRepository.DeleteCourse(courseId);

                response.Success = true;
                response.StatusCode = 200;
                response.Data = true;
                response.Message = "Xóa khóa học thành công";

                _logger.LogInformation("Course {CourseId} deleted", courseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                _logger.LogError(ex, "Error in DeleteCourseAsync for CourseId: {CourseId}", courseId);
            }

            return response;
        }
    }
}
