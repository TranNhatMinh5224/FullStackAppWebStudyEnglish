using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using AutoMapper;
using Microsoft.Extensions.Logging;
namespace LearningEnglish.Application.Service
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<LessonService> _logger;
        private readonly ITeacherPackageRepository _teacherPackageRepository;
        private readonly IMinioFileStorage _minioFileStorage;

        // Đặt bucket + folder cho ảnh lesson
        private const string LessonImageBucket = "lessons";
        private const string LessonImageFolder = "real";

        public LessonService(
            ILessonRepository lessonRepository,
            IMapper mapper,
            ILogger<LessonService> logger,
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

        // admin Thêm Lesson vào Course 
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
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                // Admin có thể thêm vào System course (không giới hạn)
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
                        response.Message = "Không thể lưu ảnh bài học. Vui lòng thử lại.";
                        return response;
                    }

                    committedImageKey = commitResult.Data;
                    lesson.ImageUrl = committedImageKey;
                    lesson.ImageType = dto.ImageType;
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
                if (!string.IsNullOrWhiteSpace(lesson.ImageUrl))
                {
                    lessonDto.ImageUrl = BuildPublicUrl.BuildURL(
                        LessonImageBucket,
                        lesson.ImageUrl
                    );
                    lessonDto.ImageType = lesson.ImageType;
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
        // teacher thêm lesson

        public async Task<ServiceResponse<LessonDto>> TeacherAddLesson(TeacherCreateLessonDto dto)
        {
            var response = new ServiceResponse<LessonDto>();
            try
            {
                var course = await _courseRepository.GetCourseById(dto.CourseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                // Chỉ teacher course mới được thêm
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
                        response.Message = "Không thể lưu ảnh bài học. Vui lòng thử lại.";
                        return response;
                    }

                    committedImageKey = commitResult.Data;
                    lesson.ImageUrl = committedImageKey;
                    lesson.ImageType = dto.ImageType;
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
                if (!string.IsNullOrWhiteSpace(lesson.ImageUrl))
                {
                    lessonDto.ImageUrl = BuildPublicUrl.BuildURL(
                        LessonImageBucket,
                        lesson.ImageUrl
                    );
                    lessonDto.ImageType = lesson.ImageType;
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
        public async Task<ServiceResponse<List<ListLessonDto>>> GetListLessonByCourseId(int CourseId, int userId, string userRole)
        {
            var response = new ServiceResponse<List<ListLessonDto>>();
            try
            {
                var course = await _courseRepository.GetCourseById(CourseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }
                if (userRole == "Admin")
                {
                    response.StatusCode = 200;
                    response.Data = (await _lessonRepository.GetListLessonByCourseId(CourseId))
                        .Select(l => _mapper.Map<ListLessonDto>(l)).ToList();
                }
                else if (userRole == "Teacher")
                {
                    if (course.Type != CourseType.Teacher || course.TeacherId != userId)
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Bạn chỉ có thể xem bài học của khóa học do mình tạo";
                        return response;
                    }
                }
                else if (userRole == "Student")
                {
                    bool isEnrolled = await _courseRepository.IsUserEnrolled(course.CourseId, userId);
                    if (!isEnrolled)
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Khóa học này mà bạn chưa đăng ký, xin vui lòng đăng ký để xem những bài học mà bạn muốn.";
                        return response;
                    }
                }
                else
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Không có quyền truy cập";
                    return response;
                }

                var lessons = await _lessonRepository.GetListLessonByCourseId(CourseId);
                var lessonDtos = lessons.Select(l => _mapper.Map<ListLessonDto>(l)).ToList();
                
                // Generate URL từ key cho tất cả lessons
                foreach (var lessonDto in lessonDtos)
                {
                    if (!string.IsNullOrWhiteSpace(lessonDto.ImageUrl))
                    {
                        lessonDto.ImageUrl = BuildPublicUrl.BuildURL(
                            LessonImageBucket,
                            lessonDto.ImageUrl
                        );
                    }
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
        public async Task<ServiceResponse<LessonDto>> GetLessonById(int lessonId, int userId, string userRole)
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


                var course = await _courseRepository.GetCourseById(lesson.CourseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }


                if (userRole == "Admin")
                {
                    response.StatusCode = 200;
                    response.Data = _mapper.Map<LessonDto>(lesson);
                }

                else if (userRole == "Teacher")
                {
                    if (course.Type != CourseType.Teacher || course.TeacherId != userId)
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Bạn chỉ có thể xem bài học của khóa học do mình tạo";
                        return response;
                    }
                }

                else if (userRole == "Student")
                {
                    bool isEnrolled = await _courseRepository.IsUserEnrolled(course.CourseId, userId);
                    if (!isEnrolled)
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Bài học này thuộc khóa học mà bạn chưa đăng ký, xin vui lòng đăng ký để xem những bài học mà bạn muốn.";
                        return response;
                    }
                }

                else
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Không có quyền truy cập";
                    return response;
                }


                var lessonDto = _mapper.Map<LessonDto>(lesson);
                
                // Generate URL từ key
                if (!string.IsNullOrWhiteSpace(lessonDto.ImageUrl))
                {
                    lessonDto.ImageUrl = BuildPublicUrl.BuildURL(
                        LessonImageBucket,
                        lessonDto.ImageUrl
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
        // cập nhật lesson
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
                    response.Message = "Không tìm thấy bài học";
                    return response;
                }

                // Cập nhật thông tin cơ bản
                lesson.Title = dto.Title;
                lesson.Description = dto.Description;
                lesson.UpdatedAt = DateTime.UtcNow;
                lesson.OrderIndex = dto.OrderIndex ?? lesson.OrderIndex;

                string? newImageKey = null;
                string? oldImageKey = !string.IsNullOrWhiteSpace(lesson.ImageUrl) ? lesson.ImageUrl : null;
                
                // Xử lý file ảnh: commit new file first
                if (!string.IsNullOrWhiteSpace(dto.ImageTempKey))
                {
                    // Commit ảnh mới
                    var commitResult = await _minioFileStorage.CommitFileAsync(
                        dto.ImageTempKey,
                        LessonImageBucket,
                        LessonImageFolder
                    );

                    if (!commitResult.Success || string.IsNullOrWhiteSpace(commitResult.Data))
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể cập nhật ảnh bài học.";
                        return response;
                    }

                    newImageKey = commitResult.Data;
                    lesson.ImageUrl = newImageKey;
                    lesson.ImageType = dto.ImageType;
                }

                try
                {
                    await _lessonRepository.UpdateLesson(lesson);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while updating lesson");
                    
                    // Rollback new image
                    if (newImageKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(newImageKey, LessonImageBucket);
                    }
                    
                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = "Lỗi database khi cập nhật bài học";
                    return response;
                }
                
                // Delete old image only after successful DB update
                if (oldImageKey != null && newImageKey != null)
                {
                    try
                    {
                        await _minioFileStorage.DeleteFileAsync(oldImageKey, LessonImageBucket);
                    }
                    catch
                    {
                        _logger.LogWarning("Failed to delete old lesson image: {ImageUrl}", oldImageKey);
                    }
                }

                // Map response và generate URL từ key
                var lessonDto = _mapper.Map<LessonDto>(lesson);
                if (!string.IsNullOrWhiteSpace(lesson.ImageUrl))
                {
                    lessonDto.ImageUrl = BuildPublicUrl.BuildURL(
                        LessonImageBucket,
                        lesson.ImageUrl
                    );
                    lessonDto.ImageType = lesson.ImageType;
                }

                response.StatusCode = 200;
                response.Message = "Cập nhật bài học thành công";
                response.Data = lessonDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lesson");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }
        public async Task<ServiceResponse<bool>> DeleteLesson(int lessonId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var lesson = await _lessonRepository.GetLessonById(lessonId);

                if (lesson == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài học";
                    response.Data = false;
                    return response;
                }
                var courseId = lesson.CourseId;
                var course = await _courseRepository.GetCourseById(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    response.Data = false;
                    return response;
                }
                switch (course.Type)
                {
                    case CourseType.System:
                        // Admin mới được xóa lesson trong System course
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Chỉ admin mới có thể xóa bài học từ khóa học hệ thống";
                        response.Data = false;
                        return response;
                    case CourseType.Teacher:
                        // Teacher mới được xóa lesson trong Teacher course
                        // Giới hạn số lượng lesson không áp dụng khi xóa
                        break;
                    default:
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Loại khóa học không hợp lệ";
                        response.Data = false;
                        return response;
                }

                // Xóa ảnh lesson trên MinIO nếu có
                if (!string.IsNullOrWhiteSpace(lesson.ImageUrl))
                {
                    try
                    {
                        await _minioFileStorage.DeleteFileAsync(
                            lesson.ImageUrl,
                            LessonImageBucket
                        );
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Failed to delete lesson image: {ImageUrl}", lesson.ImageUrl);
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
        public async Task<bool> CheckTeacherLessonPermission(int lessonId, int teacherId)
        {
            var lesson = await _lessonRepository.GetLessonById(lessonId);
            if (lesson == null)
            {
                return false;
            }

            var course = await _courseRepository.GetCourseById(lesson.CourseId);
            if (course == null || course.Type != CourseType.Teacher || course.TeacherId != teacherId)
            {
                return false;
            }

            return true;
        }

        public async Task<ServiceResponse<bool>> DeleteLesson(DeleteLessonDto dto)
        {
            return await DeleteLesson(dto.LessonId);
        }

        public async Task<ServiceResponse<bool>> DeleteLessonWithAuthorizationAsync(int lessonId, int userId, string userRole)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                // Get lesson first to check if it exists
                var lessonResponse = await GetLessonById(lessonId, userId, userRole);
                if (!lessonResponse.Success || lessonResponse.Data == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài học";
                    response.Data = false;
                    return response;
                }

                // Admin can delete any lesson
                if (userRole == "Admin")
                {
                    _logger.LogInformation("Admin {UserId} is deleting lesson {LessonId}", userId, lessonId);
                    return await DeleteLesson(lessonId);
                }

                // Teacher can only delete lessons from their own courses
                if (userRole == "Teacher")
                {
                    var hasPermission = await CheckTeacherLessonPermission(lessonId, userId);
                    if (!hasPermission)
                    {
                        _logger.LogWarning("Teacher {UserId} attempted to delete lesson {LessonId} without permission", userId, lessonId);
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Bạn chỉ có thể xóa bài học từ khóa học của mình";
                        response.Data = false;
                        return response;
                    }

                    _logger.LogInformation("Teacher {UserId} is deleting lesson {LessonId} from course {CourseId}", userId, lessonId, lessonResponse.Data.CourseId);
                    return await DeleteLesson(lessonId);
                }

                response.Success = false;
                response.StatusCode = 403;
                response.Message = "Không có quyền truy cập";
                response.Data = false;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteLessonWithAuthorizationAsync for lesson {LessonId} by user {UserId}", lessonId, userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                response.Data = false;
                return response;
            }
        }

        public async Task<ServiceResponse<LessonDto>> UpdateLessonWithAuthorizationAsync(int lessonId, UpdateLessonDto dto, int userId, string userRole)
        {
            var response = new ServiceResponse<LessonDto>();
            try
            {
                // Get lesson first to check if it exists
                var lessonResponse = await GetLessonById(lessonId, userId, userRole);
                if (!lessonResponse.Success || lessonResponse.Data == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài học";
                    return response;
                }

                // Admin can update any lesson
                if (userRole == "Admin")
                {
                    _logger.LogInformation("Admin {UserId} is updating lesson {LessonId}", userId, lessonId);
                    return await UpdateLesson(lessonId, dto);
                }

                // Teacher can only update lessons from their own courses
                if (userRole == "Teacher")
                {
                    var hasPermission = await CheckTeacherLessonPermission(lessonId, userId);
                    if (!hasPermission)
                    {
                        _logger.LogWarning("Teacher {UserId} attempted to update lesson {LessonId} without permission", userId, lessonId);
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Bạn chỉ có thể cập nhật bài học từ khóa học của mình";
                        return response;
                    }

                    _logger.LogInformation("Teacher {UserId} is updating lesson {LessonId}", userId, lessonId);
                    return await UpdateLesson(lessonId, dto);
                }

                response.Success = false;
                response.StatusCode = 403;
                response.Message = "Không có quyền truy cập";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateLessonWithAuthorizationAsync for lesson {LessonId} by user {UserId}", lessonId, userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                return response;
            }
        }
    }
}
