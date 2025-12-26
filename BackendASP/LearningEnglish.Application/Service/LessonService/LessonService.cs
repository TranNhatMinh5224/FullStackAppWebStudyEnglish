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
        private readonly ILessonCompletionRepository _lessonCompletionRepository;

        // Đặt bucket + folder cho ảnh lesson
        private const string LessonImageBucket = "lessons";
        private const string LessonImageFolder = "real";

        public LessonService(
            ILessonRepository lessonRepository,
            IMapper mapper,
            ILogger<LessonService> logger,
            ICourseRepository courseRepository,
            ITeacherPackageRepository teacherPackageRepository,
            IMinioFileStorage minioFileStorage,
            ILessonCompletionRepository lessonCompletionRepository)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _logger = logger;
            _courseRepository = courseRepository;
            _teacherPackageRepository = teacherPackageRepository;
            _minioFileStorage = minioFileStorage;
            _lessonCompletionRepository = lessonCompletionRepository;
        }

        // Admin thêm Lesson vào Course
        // RLS: lessons_policy_admin_all sẽ check permission Admin.Lesson.Manage khi INSERT
        public async Task<ServiceResponse<LessonDto>> AdminAddLesson(AdminCreateLessonDto dto)
        {
            var response = new ServiceResponse<LessonDto>();
            try
            {
                // RLS đã filter courses theo permission Admin.Course.Manage
                // Nếu course không tồn tại hoặc không có quyền → RLS sẽ filter → course == null
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
                        response.Message = "Không thể lưu ảnh bài học. Vui lòng thử lại.";
                        return response;
                    }

                    committedImageKey = commitResult.Data;
                    lesson.ImageKey = committedImageKey;
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
                if (!string.IsNullOrWhiteSpace(lesson.ImageKey))
                {
                    lessonDto.ImageUrl = BuildPublicUrl.BuildURL(
                        LessonImageBucket,
                        lesson.ImageKey
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
        // Teacher thêm lesson
        // RLS: lessons_policy_teacher_all_own sẽ check course ownership khi INSERT
        // Defense in depth: Check ownership ở service layer để có error message rõ ràng
        public async Task<ServiceResponse<LessonDto>> TeacherAddLesson(TeacherCreateLessonDto dto, int userId)
        {
            var response = new ServiceResponse<LessonDto>();
            try
            {
                // RLS đã filter courses theo TeacherId (chỉ courses của teacher này)
                // Nếu course không tồn tại hoặc không thuộc về teacher → RLS sẽ filter → course == null
                var course = await _courseRepository.GetCourseById(dto.CourseId);
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

                // Defense in depth: Check ownership ở service layer (RLS cũng sẽ block nếu không đúng)
                // Giúp trả về error message rõ ràng hơn trước khi INSERT
                if (!course.TeacherId.HasValue || course.TeacherId.Value != userId)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền thêm bài học vào khóa học này";
                    _logger.LogWarning("Teacher {UserId} attempted to add lesson to course {CourseId} owned by {OwnerId}",
                        userId, dto.CourseId, course.TeacherId);
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
                    lesson.ImageKey = committedImageKey;
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
                if (!string.IsNullOrWhiteSpace(lesson.ImageKey))
                {
                    lessonDto.ImageUrl = BuildPublicUrl.BuildURL(
                        LessonImageBucket,
                        lesson.ImageKey
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
        public async Task<ServiceResponse<List<LessonWithProgressDto>>> GetListLessonByCourseId(int courseId, int? userId = null)
        {
            var response = new ServiceResponse<List<LessonWithProgressDto>>();
            try
            {
                // RLS đã tự động filter courses và lessons theo role:
                // - Admin: thấy tất cả courses/lessons
                // - Teacher: chỉ thấy own courses/lessons
                // - Student: chỉ thấy enrolled courses/lessons
                // Nếu course không tồn tại hoặc không có quyền → RLS sẽ filter → course == null
                var course = await _courseRepository.GetCourseById(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                // RLS policy lessons_policy_* đã tự động filter lessons theo role
                // Nếu không có quyền, GetListLessonByCourseId sẽ trả về empty list
                var lessons = await _lessonRepository.GetListLessonByCourseId(courseId);
                var lessonDtos = new List<LessonWithProgressDto>();

                // Map lessons with progress (for Students) or without progress (for Admin/Teacher)
                foreach (var lesson in lessons)
                {
                    var lessonDto = new LessonWithProgressDto
                    {
                        LessonId = lesson.LessonId,
                        Title = lesson.Title,
                        Description = lesson.Description,
                        OrderIndex = lesson.OrderIndex,
                        CourseId = lesson.CourseId,
                        ImageType = lesson.ImageType
                    };

                    // Generate image URL
                    if (!string.IsNullOrWhiteSpace(lesson.ImageKey))
                    {
                        lessonDto.ImageUrl = BuildPublicUrl.BuildURL(
                            LessonImageBucket,
                            lesson.ImageKey
                        );
                    }

                    // ✅ Add progress info for Students (nếu có userId)
                    if (userId.HasValue)
                    {
                        var lessonCompletion = await _lessonCompletionRepository.GetByUserAndLessonAsync(userId.Value, lesson.LessonId);
                        if (lessonCompletion != null)
                        {
                            lessonDto.CompletionPercentage = lessonCompletion.CompletionPercentage;
                            lessonDto.IsCompleted = lessonCompletion.IsCompleted;
                            lessonDto.CompletedModules = lessonCompletion.CompletedModules;
                            lessonDto.TotalModules = lessonCompletion.TotalModules;
                            lessonDto.VideoProgressPercentage = lessonCompletion.VideoProgressPercentage;
                            lessonDto.StartedAt = lessonCompletion.StartedAt;
                            lessonDto.CompletedAt = lessonCompletion.CompletedAt;
                        }
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
        public async Task<ServiceResponse<LessonDto>> GetLessonById(int lessonId)
        {
            var response = new ServiceResponse<LessonDto>();
            try
            {
                // RLS policy lessons_policy_* đã tự động filter lessons theo role
                // Nếu lesson == null → không tồn tại hoặc không có quyền truy cập (RLS đã filter)
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

                // Note: Progress info should be retrieved using GetLessonsWithProgressByCourseIdAsync method
                // This method returns basic LessonDto without progress information

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
        // Cập nhật lesson
        // RLS: lessons_policy_* sẽ filter lessons theo role/permission khi UPDATE
        // Nếu lesson không tồn tại hoặc không có quyền → RLS sẽ filter → lesson == null
        public async Task<ServiceResponse<LessonDto>> UpdateLesson(int lessonId, UpdateLessonDto dto)
        {
            var response = new ServiceResponse<LessonDto>();
            try
            {
                // RLS đã filter lessons theo role:
                // - Admin: Tất cả lessons (có permission)
                // - Teacher: Chỉ lessons của own courses
                // - Student: Không có quyền UPDATE
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

                string? newImageKey = null;
                string? oldImageKey = !string.IsNullOrWhiteSpace(lesson.ImageKey) ? lesson.ImageKey : null;

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
                    lesson.ImageKey = newImageKey;
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
                if (!string.IsNullOrWhiteSpace(lesson.ImageKey))
                {
                    lessonDto.ImageUrl = BuildPublicUrl.BuildURL(
                        LessonImageBucket,
                        lesson.ImageKey
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
        // Xóa lesson
        // RLS: lessons_policy_* sẽ filter lessons theo role/permission khi DELETE
        // - Admin: Có thể xóa tất cả lessons (có permission)
        // - Teacher: Chỉ xóa được lessons của own courses
        // - Student: Không có quyền DELETE
        public async Task<ServiceResponse<bool>> DeleteLesson(int lessonId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                // RLS đã filter lessons theo role:
                // - Admin: Tất cả lessons (có permission)
                // - Teacher: Chỉ lessons của own courses
                // Nếu lesson không tồn tại hoặc không có quyền → RLS sẽ filter → lesson == null
                var lesson = await _lessonRepository.GetLessonById(lessonId);
                if (lesson == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài học hoặc bạn không có quyền truy cập";
                    response.Data = false;
                    return response;
                }

                // Business logic: Kiểm tra course Type để phân biệt System vs Teacher course
                // RLS đã đảm bảo quyền truy cập, đây chỉ là business logic validation
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

                // Business logic: Phân biệt System course vs Teacher course
                // RLS đã filter, đây chỉ là validation để trả về error message phù hợp
                switch (course.Type)
                {
                    case CourseType.System:
                        // Admin mới được xóa lesson trong System course (RLS đã check permission)
                        // Nếu không phải Admin → RLS đã block → lesson == null → đã return 404 ở trên
                        break;
                    case CourseType.Teacher:
                        // Teacher chỉ xóa được lessons của own courses (RLS đã check ownership)
                        // Nếu không phải owner → RLS đã block → lesson == null → đã return 404 ở trên
                        break;
                    default:
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Loại khóa học không hợp lệ";
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

        public async Task<ServiceResponse<bool>> DeleteLesson(DeleteLessonDto dto)
        {
            return await DeleteLesson(dto.LessonId);
        }

        // ✅ NEW: Get lessons with progress for students
        public async Task<ServiceResponse<List<LessonWithProgressDto>>> GetLessonsWithProgressByCourseIdAsync(int courseId, int userId)
        {
            var response = new ServiceResponse<List<LessonWithProgressDto>>();
            try
            {
                // Check if course exists
                var course = await _courseRepository.GetCourseById(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                // Check if user is enrolled
                bool isEnrolled = await _courseRepository.IsUserEnrolled(courseId, userId);
                if (!isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn chưa đăng ký khóa học này";
                    return response;
                }

                // Get all lessons for the course
                var lessons = await _lessonRepository.GetListLessonByCourseId(courseId);
                var lessonWithProgressDtos = new List<LessonWithProgressDto>();

                foreach (var lesson in lessons)
                {
                    var lessonDto = new LessonWithProgressDto
                    {
                        LessonId = lesson.LessonId,
                        Title = lesson.Title,
                        Description = lesson.Description,
                        OrderIndex = lesson.OrderIndex,
                        CourseId = lesson.CourseId,
                        ImageType = lesson.ImageType
                    };

                    // Generate image URL
                    if (!string.IsNullOrWhiteSpace(lesson.ImageKey))
                    {
                        lessonDto.ImageUrl = BuildPublicUrl.BuildURL(
                            LessonImageBucket,
                            lesson.ImageKey
                        );
                    }

                    // ✅ Get progress information for this lesson
                    var lessonCompletion = await _lessonCompletionRepository.GetByUserAndLessonAsync(userId, lesson.LessonId);

                    if (lessonCompletion != null)
                    {
                        lessonDto.CompletionPercentage = lessonCompletion.CompletionPercentage;
                        lessonDto.IsCompleted = lessonCompletion.IsCompleted;
                        lessonDto.CompletedModules = lessonCompletion.CompletedModules;
                        lessonDto.TotalModules = lessonCompletion.TotalModules;
                        lessonDto.VideoProgressPercentage = lessonCompletion.VideoProgressPercentage;
                        lessonDto.StartedAt = lessonCompletion.StartedAt;
                        lessonDto.CompletedAt = lessonCompletion.CompletedAt;
                    }
                    else
                    {
                        // No progress yet, set default values
                        lessonDto.CompletionPercentage = 0;
                        lessonDto.IsCompleted = false;
                        lessonDto.CompletedModules = 0;
                        lessonDto.TotalModules = 0; // TODO: Could query module count if needed
                        lessonDto.VideoProgressPercentage = 0;
                        lessonDto.StartedAt = null;
                        lessonDto.CompletedAt = null;
                    }

                    lessonWithProgressDtos.Add(lessonDto);
                }

                response.Data = lessonWithProgressDtos;
                response.Message = "Lấy danh sách lesson với tiến độ thành công";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lessons với tiến độ cho course {CourseId}, user {UserId}", courseId, userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi lấy danh sách lesson với tiến độ";
                return response;
            }
        }
    }
}
