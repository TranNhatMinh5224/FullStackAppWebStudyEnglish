using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Domain.Entities;
using CleanDemo.Domain.Enums;
using CleanDemo.Application.Common;
using AutoMapper;
using Microsoft.Extensions.Logging;
namespace CleanDemo.Application.Service
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<LessonService> _logger;
        private readonly ITeacherPackageRepository _teacherPackageRepository;

        public LessonService(
            ILessonRepository lessonRepository,
            IMapper mapper,
            ILogger<LessonService> logger,
            ICourseRepository courseRepository,
            ITeacherPackageRepository teacherPackageRepository)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _logger = logger;
            _courseRepository = courseRepository;
            _teacherPackageRepository = teacherPackageRepository;
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
                await _lessonRepository.AddLesson(lesson);
                response.StatusCode = 201;
                response.Message = "Tạo bài học thành công";
                response.Data = _mapper.Map<LessonDto>(lesson);
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
                await _lessonRepository.AddLesson(lesson);
                response.StatusCode = 201;
                response.Message = "Tạo bài học thành công";
                response.Data = _mapper.Map<LessonDto>(lesson);
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
        public async Task<ServiceResponse<List<ListLessonDto>>> GetListLessonByCourseId(int CourseId)
        {
            var response = new ServiceResponse<List<ListLessonDto>>();
            try
            {
                var lessons = await _lessonRepository.GetListLessonByCourseId(CourseId);
                response.StatusCode = 200;
                response.Data = lessons.Select(l => _mapper.Map<ListLessonDto>(l)).ToList();
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
                var lesson = await _lessonRepository.GetLessonById(lessonId);
                if (lesson == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài học";
                    return response;
                }
                response.StatusCode = 200;
                response.Data = _mapper.Map<LessonDto>(lesson);
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

                // Cập nhật trực tiếp entity đã tồn tại 
                lesson.Title = dto.Title;
                lesson.Description = dto.Description;

                await _lessonRepository.UpdateLesson(lesson);
                response.StatusCode = 200;
                response.Message = "Cập nhật bài học thành công";
                response.Data = _mapper.Map<LessonDto>(lesson);
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
                var lessonResponse = await GetLessonById(lessonId);
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
                var lessonResponse = await GetLessonById(lessonId);
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
