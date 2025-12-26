using LearningEnglish.Application.DTOs;

using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.Common.Pagination;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

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

        // Lấy danh sách loại khóa học (System/Teacher) - Dùng cho giao diện quản lý Admin để filter
        // Tuân thủ CLEAN + SOLID: Service gọi Repository, không xử lý business logic trực tiếp
        public async Task<ServiceResponse<IEnumerable<CourseTypeDto>>> GetCourseTypesAsync()
        {
            var response = new ServiceResponse<IEnumerable<CourseTypeDto>>();
            
            try
            {
                // Gọi Repository để lấy data (tuân thủ Dependency Inversion Principle)
                var courseTypes = await _courseRepository.GetCourseTypesAsync();

                response.Success = true;
                response.StatusCode = 200;
                response.Data = courseTypes;
                response.Message = "Lấy danh sách loại khóa học thành công";
                
                _logger.LogInformation("Retrieved {Count} course types", courseTypes.Count());
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi lấy danh sách loại khóa học";
                _logger.LogError(ex, "Error in GetCourseTypesAsync");
            }

            return response;
        }






        // Service cho Admin Lấy ra toàn bộ khóa học với phân trang - Sort theo Title mặc định
        public async Task<ServiceResponse<PagedResult<AdminCourseListResponseDto>>> GetAllCoursesPagedAsync(AdminCourseQueryParameters parameters)
        {
            var response = new ServiceResponse<PagedResult<AdminCourseListResponseDto>>();
            try
            {
                var pagedData = await _courseRepository.GetAllCoursesPagedForAdminAsync(parameters);

                var items = new List<AdminCourseListResponseDto>();
                foreach (var course in pagedData.Items)
                {
                    var dto = _mapper.Map<AdminCourseListResponseDto>(course);
                    dto.LessonCount = await _courseRepository.CountLessons(course.CourseId);
                    dto.StudentCount = await _courseRepository.CountEnrolledUsers(course.CourseId);
                    dto.TeacherName = course.Teacher?.FirstName + " " + course.Teacher?.LastName ?? "System Admin";

                    if (!string.IsNullOrWhiteSpace(course.ImageKey))
                    {
                        dto.ImageUrl = BuildPublicUrl.BuildURL(CourseImageBucket, course.ImageKey);
                        dto.ImageType = course.ImageType;
                    }
                    items.Add(dto);
                }

                response.StatusCode = 200;
                response.Data = new PagedResult<AdminCourseListResponseDto>
                {
                    Items = items,
                    TotalCount = pagedData.TotalCount,
                    PageNumber = pagedData.PageNumber,
                    PageSize = pagedData.PageSize
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                _logger.LogError(ex, "Error");
            }
            return response;
        }

        // Service cho Admin Tạo mới khóa học

        public async Task<ServiceResponse<CourseResponseDto>> AdminCreateCourseAsync(AdminCreateCourseRequestDto requestDto)
        {
            var response = new ServiceResponse<CourseResponseDto>();


            try
            {
                // Tạo course entity
                var course = new Course
                {
                    Title = requestDto.Title,
                    DescriptionMarkdown = requestDto.Description,
                    Type = requestDto.Type,
                    Price = requestDto.Price,
                    TeacherId = null,
                    MaxStudent = requestDto.MaxStudent,
                    IsFeatured = requestDto.IsFeatured,
                    EnrollmentCount = 0
                };

                string? committedImageKey = null; // Biến lưu trữ key của ảnh đã commit  

                // Convert temp file → real file nếu có ImageTempKey
                if (!string.IsNullOrWhiteSpace(requestDto.ImageTempKey))  // nếu có ảnh tạm
                {
                    var commitResult = await _minioFileStorage.CommitFileAsync(
                        requestDto.ImageTempKey,
                        CourseImageBucket,
                        CourseImageFolder
                    );

                    if (!commitResult.Success || string.IsNullOrWhiteSpace(commitResult.Data)) // nếu commit không thành công or trả về key rỗng
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu ảnh khóa học. Vui lòng thử lại.";
                        return response;
                    }


                    committedImageKey = commitResult.Data; // Lưu key ảnh đã commit
                    course.ImageKey = committedImageKey; // Gán key ảnh cho course
                    course.ImageType = requestDto.ImageType; // Gán kiểu ảnh cho course
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
                if (!string.IsNullOrWhiteSpace(course.ImageKey))
                {
                    courseResponseDto.ImageUrl = BuildPublicUrl.BuildURL(
                        CourseImageBucket,
                        course.ImageKey
                    );
                    courseResponseDto.ImageType = course.ImageType;
                }

                response.StatusCode = 201;
                response.Data = courseResponseDto;
                response.Message = "Tạo khóa học thành công";
                response.Success = true;

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





        // Service cho Admin Cập nhật khóa học

        public async Task<ServiceResponse<CourseResponseDto>> AdminUpdateCourseAsync(int courseId, AdminUpdateCourseRequestDto requestDto)
        {
            var response = new ServiceResponse<CourseResponseDto>();

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

                if (!string.IsNullOrWhiteSpace(requestDto.Title))
                    course.Title = requestDto.Title;

                if (!string.IsNullOrWhiteSpace(requestDto.Description))
                    course.DescriptionMarkdown = requestDto.Description;

                if (requestDto.Price.HasValue)
                    course.Price = requestDto.Price.Value;

                if (requestDto.MaxStudent.HasValue && requestDto.MaxStudent.Value > 0)
                    course.MaxStudent = requestDto.MaxStudent.Value;

                if (requestDto.IsFeatured.HasValue)
                    course.IsFeatured = requestDto.IsFeatured.Value;

                if (requestDto.Type.HasValue)
                    course.Type = requestDto.Type.Value;

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

                // Delete key ảnh cũ 
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
                courseResponseDto.TeacherName = course.Teacher != null
                   ? $"{course.Teacher.FirstName} {course.Teacher.LastName}"
                   : "System Admin";

                // Generate URL từ key
                if (!string.IsNullOrWhiteSpace(course.ImageKey))
                {
                    courseResponseDto.ImageUrl = BuildPublicUrl.BuildURL(
                        CourseImageBucket,
                        course.ImageKey
                    );
                    courseResponseDto.ImageType = course.ImageType;
                }


                response.StatusCode = 200;
                response.Data = courseResponseDto;
                response.Success = true;
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


        // Service cho Admin Xóa khóa học

        public async Task<ServiceResponse<bool>> DeleteCourseAsync(int courseId)
        {
            var response = new ServiceResponse<bool>();

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

                // Xóa ảnh khóa học trên MinIO nếu có
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
