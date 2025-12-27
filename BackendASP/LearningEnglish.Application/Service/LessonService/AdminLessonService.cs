using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Lesson;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class AdminLessonService : IAdminLessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<AdminLessonService> _logger;
        private readonly IMinioFileStorage _minioFileStorage;

        // Đặt bucket + folder cho ảnh lesson
        private const string LessonImageBucket = "lessons";
        private const string LessonImageFolder = "real";

        public AdminLessonService(
            ILessonRepository lessonRepository,
            IMapper mapper,
            ILogger<AdminLessonService> logger,
            ICourseRepository courseRepository,
            IMinioFileStorage minioFileStorage)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _logger = logger;
            _courseRepository = courseRepository;
            _minioFileStorage = minioFileStorage;
        }

        // Admin thêm Lesson vào Course
       
        public async Task<ServiceResponse<LessonDto>> AdminAddLesson(AdminCreateLessonDto dto)
        {
            var response = new ServiceResponse<LessonDto>();
            try
            {
       
                var course = await _courseRepository.GetCourseById(dto.CourseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học hoặc bạn không có quyền truy cập";
                    return response;
                }

                // Business logic: Admin chỉ thêm vào System course
                if (course.Type != CourseType.System)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ admin mới có thể thêm bài học vào khóa học hệ thống";
                    return response;
                }

                // check tên Lesson đã tồn tại trong Course chưa
                var lessons = await _lessonRepository.LessonIncourse(dto.Title, dto.CourseId);
                if (lessons)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Bài học đã tồn tại trong khóa học này";
                    return response;
                }

                var lesson = new Lesson
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    CourseId = dto.CourseId
                };

                string? committedImageKey = null;

                // Convert temp file → real file nếu có ImageTempKey
                if (!string.IsNullOrWhiteSpace(dto.ImageTempKey))
                {
                    var commitResult = await _minioFileStorage.CommitFileAsync(
                        dto.ImageTempKey,
                        LessonImageBucket,
                        LessonImageFolder
                    );

                    if (!commitResult.Success || string.IsNullOrWhiteSpace(commitResult.Data))
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = commitResult.Message ?? "Không thể commit file";
                        return response;
                    }

                    committedImageKey = commitResult.Data;
                    lesson.ImageKey = committedImageKey;
                    lesson.ImageType = dto.ImageType ?? "real";
                }

                // RLS sẽ check permission Admin.Lesson.Manage khi INSERT
                await _lessonRepository.AddLesson(lesson);

                var lessonDto = _mapper.Map<LessonDto>(lesson);

                // Generate URL từ key
                if (!string.IsNullOrWhiteSpace(lesson.ImageKey))
                {
                    lessonDto.ImageUrl = BuildPublicUrl.BuildURL(
                        LessonImageBucket,
                        lesson.ImageKey
                    );
                }

                response.StatusCode = 201;
                response.Data = lessonDto;
                response.Message = "Tạo bài học thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding lesson");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        // Cập nhật lesson
      
        public async Task<ServiceResponse<LessonDto>> UpdateLesson(int lessonId, UpdateLessonDto dto)
        {
            var response = new ServiceResponse<LessonDto>();
            try
            {
               
                var lesson = await _lessonRepository.GetLessonById(lessonId);
                if (lesson == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài học hoặc bạn không có quyền truy cập";
                    return response;
                }

                // Cập nhật thông tin cơ bản
                lesson.Title = dto.Title;
                lesson.Description = dto.Description;
                lesson.UpdatedAt = DateTime.UtcNow;
                lesson.OrderIndex = dto.OrderIndex ?? lesson.OrderIndex;

                string? oldImageKey = lesson.ImageKey;
                string? newImageKey = null;

                // Xử lý ảnh mới (nếu có)
                if (!string.IsNullOrWhiteSpace(dto.ImageTempKey))
                {
                    var commitResult = await _minioFileStorage.CommitFileAsync(
                        dto.ImageTempKey,
                        LessonImageBucket,
                        LessonImageFolder
                    );

                    if (!commitResult.Success || string.IsNullOrWhiteSpace(commitResult.Data))
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = commitResult.Message ?? "Không thể commit file mới";
                        return response;
                    }

                    newImageKey = commitResult.Data;
                    lesson.ImageKey = newImageKey;
                    lesson.ImageType = dto.ImageType ?? "real";
                }

                // RLS sẽ check permission khi UPDATE
                await _lessonRepository.UpdateLesson(lesson);

                // Xóa ảnh cũ nếu có ảnh mới
                if (!string.IsNullOrWhiteSpace(oldImageKey) && !string.IsNullOrWhiteSpace(newImageKey))
                {
                    try
                    {
                        await _minioFileStorage.DeleteFileAsync(oldImageKey, LessonImageBucket);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Failed to delete old lesson image: {ImageUrl}", oldImageKey);
                    }
                }

                var lessonDto = _mapper.Map<LessonDto>(lesson);

                // Generate URL từ key
                if (!string.IsNullOrWhiteSpace(lesson.ImageKey))
                {
                    lessonDto.ImageUrl = BuildPublicUrl.BuildURL(
                        LessonImageBucket,
                        lesson.ImageKey
                    );
                }

                response.StatusCode = 200;
                response.Data = lessonDto;
                response.Message = "Cập nhật bài học thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lesson {LessonId}", lessonId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        // Xóa lesson

        public async Task<ServiceResponse<bool>> DeleteLesson(int lessonId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                // RLS đã filter lessons theo role: Admin có thể xóa tất cả lessons (có permission)
                var lesson = await _lessonRepository.GetLessonById(lessonId);
                if (lesson == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài học hoặc bạn không có quyền truy cập";
                    response.Data = false;
                    return response;
                }

                // Xóa ảnh lesson trên MinIO nếu có
                if (!string.IsNullOrWhiteSpace(lesson.ImageKey))
                {
                    try
                    {
                        await _minioFileStorage.DeleteFileAsync(
                            lesson.ImageKey,
                            LessonImageBucket
                        );
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Failed to delete lesson image: {ImageUrl}", lesson.ImageKey);
                    }
                }

                await _lessonRepository.DeleteLesson(lessonId);
                response.StatusCode = 200;
                response.Message = "Xóa bài học thành công";
                response.Data = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lesson {LessonId}", lessonId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                response.Data = false;
            }
            return response;
        }

        // Get lesson by ID (read-only)
        public async Task<ServiceResponse<LessonDto>> GetLessonById(int lessonId)
        {
            var response = new ServiceResponse<LessonDto>();
            try
            {
                var lesson = await _lessonRepository.GetLessonById(lessonId);
                if (lesson == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài học";
                    return response;
                }

                var lessonDto = _mapper.Map<LessonDto>(lesson);

                // Generate URL từ key
                if (!string.IsNullOrWhiteSpace(lesson.ImageKey))
                {
                    lessonDto.ImageUrl = BuildPublicUrl.BuildURL(
                        LessonImageBucket,
                        lesson.ImageKey
                    );
                }

                response.StatusCode = 200;
                response.Data = lessonDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lesson");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        // Get list lessons by courseId (without progress)
        public async Task<ServiceResponse<List<LessonDto>>> GetListLessonByCourseId(int courseId)
        {
            var response = new ServiceResponse<List<LessonDto>>();
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

                var lessons = await _lessonRepository.GetListLessonByCourseId(courseId);
                var lessonDtos = new List<LessonDto>();

                foreach (var lesson in lessons)
                {
                    var lessonDto = _mapper.Map<LessonDto>(lesson);

                    // Generate image URL
                    if (!string.IsNullOrWhiteSpace(lesson.ImageKey))
                    {
                        lessonDto.ImageUrl = BuildPublicUrl.BuildURL(
                            LessonImageBucket,
                            lesson.ImageKey
                        );
                    }

                    lessonDtos.Add(lessonDto);
                }

                response.StatusCode = 200;
                response.Data = lessonDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lessons");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }
    }
}
