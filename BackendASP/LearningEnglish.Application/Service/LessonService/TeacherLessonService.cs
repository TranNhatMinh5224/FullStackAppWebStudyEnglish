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
    public class TeacherLessonService : ITeacherLessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<TeacherLessonService> _logger;
        private readonly ITeacherPackageRepository _teacherPackageRepository;
        private readonly IMinioFileStorage _minioFileStorage;

        // Đặt bucket + folder cho ảnh lesson
        private const string LessonImageBucket = "lessons";
        private const string LessonImageFolder = "real";

        public TeacherLessonService(
            ILessonRepository lessonRepository,
            IMapper mapper,
            ILogger<TeacherLessonService> logger,
            ICourseRepository courseRepository,
            ITeacherPackageRepository teacherPackageRepository,
            IMinioFileStorage minioFileStorage)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _logger = logger;
            _courseRepository = courseRepository;
            _teacherPackageRepository = teacherPackageRepository;
            _minioFileStorage = minioFileStorage;
        }

        // Teacher thêm lesson
        
        public async Task<ServiceResponse<LessonDto>> TeacherAddLesson(TeacherCreateLessonDto dto, int teacherId)
        {
            var response = new ServiceResponse<LessonDto>();
            try
            {
               
                var course = await _courseRepository.GetCourseByIdForTeacher(dto.CourseId, teacherId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học hoặc bạn không có quyền truy cập";
                    return response;
                }

                // Business logic: Chỉ teacher course mới được thêm lesson
                if (course.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể thêm bài học vào khóa học của giáo viên";
                    return response;
                }

                // Kiểm tra giới hạn số lượng lesson
                if (course.TeacherId.HasValue)
                {
                    var teacherPackage = await _teacherPackageRepository.GetInformationTeacherpackage(course.TeacherId.Value);
                    if (teacherPackage == null)
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Giáo viên không có gói đăng ký hoạt động";
                        return response;
                    }

                    int currentLessonCount = await _courseRepository.CountLessons(dto.CourseId);
                    int maxLessons = teacherPackage.MaxLessons;

                    if (currentLessonCount >= maxLessons)
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = $"Đã đạt số lượng bài học tối đa ({currentLessonCount}/{maxLessons}). Vui lòng nâng cấp gói.";
                        return response;
                    }
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

                try
                {
                    await _lessonRepository.AddLesson(lesson);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while creating lesson");

                    // Rollback MinIO file
                    if (committedImageKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(committedImageKey, LessonImageBucket);
                    }

                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = "Lỗi database khi tạo bài học";
                    return response;
                }

                // Map response và generate URL từ key
                var lessonDto = _mapper.Map<LessonDto>(lesson);
                if (!string.IsNullOrWhiteSpace(lesson.ImageKey))
                {
                    lessonDto.ImageUrl = BuildPublicUrl.BuildURL(
                        LessonImageBucket,
                        lesson.ImageKey
                    );
                }

                response.StatusCode = 201;
                response.Message = "Tạo bài học thành công";
                response.Data = lessonDto;
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
     
        public async Task<ServiceResponse<LessonDto>> UpdateLesson(int lessonId, UpdateLessonDto dto, int teacherId)
        {
            var response = new ServiceResponse<LessonDto>();
            try
            {
                
                var lesson = await _lessonRepository.GetLessonByIdForTeacher(lessonId, teacherId);
                if (lesson == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài học hoặc bạn không có quyền truy cập";
                    return response;
                }

                // Validate ownership: teacher phải sở hữu course của lesson
                var course = await _courseRepository.GetCourseByIdForTeacher(lesson.CourseId, teacherId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền cập nhật bài học này";
                    return response;
                }

                // Business logic: Chỉ teacher course mới được cập nhật lesson
                if (course.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể cập nhật bài học của khóa học giáo viên";
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
       
        public async Task<ServiceResponse<bool>> DeleteLesson(int lessonId, int teacherId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
               
                var lesson = await _lessonRepository.GetLessonByIdForTeacher(lessonId, teacherId);
                if (lesson == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài học hoặc bạn không có quyền truy cập";
                    response.Data = false;
                    return response;
                }

                // Validate ownership: teacher phải sở hữu course của lesson
                var course = await _courseRepository.GetCourseByIdForTeacher(lesson.CourseId, teacherId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền xóa bài học này";
                    response.Data = false;
                    return response;
                }

                // Business logic: Chỉ teacher course mới được xóa lesson
                if (course.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể xóa bài học của khóa học giáo viên";
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

        // Get lesson by ID (read-only) - Teacher có thể xem nếu là owner HOẶC đã enroll
        public async Task<ServiceResponse<LessonDto>> GetLessonById(int lessonId, int teacherId)
        {
            var response = new ServiceResponse<LessonDto>();
            try
            {
                // Lấy lesson với course để check
                var lesson = await _lessonRepository.GetLessonById(lessonId);
                if (lesson == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài học";
                    return response;
                }

                // Check: teacher phải là owner HOẶC đã enroll
                var course = await _courseRepository.GetCourseById(lesson.CourseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var isOwner = course.TeacherId.HasValue && course.TeacherId.Value == teacherId;
                var isEnrolled = await _courseRepository.IsUserEnrolled(lesson.CourseId, teacherId);

                if (!isOwner && !isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn cần sở hữu hoặc đăng ký khóa học để xem bài học này";
                    _logger.LogWarning("Teacher {TeacherId} attempted to access lesson {LessonId} without ownership or enrollment", 
                        teacherId, lessonId);
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

        // Get list lessons by courseId (without progress) - Teacher có thể xem nếu là owner HOẶC đã enroll
        public async Task<ServiceResponse<List<LessonDto>>> GetListLessonByCourseId(int courseId, int teacherId)
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

                // Check: teacher phải là owner HOẶC đã enroll
                var isOwner = course.TeacherId.HasValue && course.TeacherId.Value == teacherId;
                var isEnrolled = await _courseRepository.IsUserEnrolled(courseId, teacherId);

                if (!isOwner && !isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn cần sở hữu hoặc đăng ký khóa học để xem các bài học";
                    _logger.LogWarning("Teacher {TeacherId} attempted to list lessons of course {CourseId} without ownership or enrollment",
                        teacherId, courseId);
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

                response.Success = true;
                response.StatusCode = 200;
                response.Data = lessonDtos;
                response.Message = "Lấy danh sách bài học thành công";
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
