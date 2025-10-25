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
                    response.Message = "Course not found";
                    return response;
                }

                // Admin có thể thêm vào System course (không giới hạn)
                if (course.Type != CourseType.System)
                {
                    response.Success = false;
                    response.Message = "Admin can only add lessons to System courses";
                    return response;
                }

                // check tên Lesson đã tồn tại trong Course chưa
                var lessons = await _lessonRepository.LessonIncourse(dto.Title, dto.CourseId);
                if (lessons)
                {
                    response.Success = false;
                    response.Message = "Lesson already exists in this course";
                    return response;
                }
                var lesson = new Lesson
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    CourseId = dto.CourseId
                };
                await _lessonRepository.AddLesson(lesson);
                response.Data = _mapper.Map<LessonDto>(lesson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding lesson");
                response.Success = false;
                response.Message = "Error adding lesson";
                return response;
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
                    response.Message = "Course not found";
                    return response;
                }

                // Chỉ teacher course mới được thêm
                if (course.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.Message = "Can only add lessons to Teacher courses";
                    return response;
                }

                // Kiểm tra giới hạn số lượng lesson
                if (course.TeacherId.HasValue)
                {
                    var teacherPackage = await _teacherPackageRepository.GetInformationTeacherpackage(course.TeacherId.Value);
                    if (teacherPackage == null)
                    {
                        response.Success = false;
                        response.Message = "Teacher does not have an active subscription";
                        return response;
                    }

                    int currentLessonCount = await _courseRepository.CountLessons(dto.CourseId);
                    int maxLessons = teacherPackage.MaxLessons;

                    if (currentLessonCount >= maxLessons)
                    {
                        response.Success = false;
                        response.Message = $"Maximum lessons reached ({currentLessonCount}/{maxLessons}). Please upgrade your package.";
                        return response;
                    }
                }

                // check tên Lesson đã tồn tại trong Course chưa
                var lessons = await _lessonRepository.LessonIncourse(dto.Title, dto.CourseId);
                if (lessons)
                {
                    response.Success = false;
                    response.Message = "Lesson already exists in this course";
                    return response;
                }

                var lesson = new Lesson
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    CourseId = dto.CourseId
                };
                await _lessonRepository.AddLesson(lesson);
                response.Data = _mapper.Map<LessonDto>(lesson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding lesson");
                response.Success = false;
                response.Message = "Error adding lesson";
                return response;
            }
            return response;


        }
        public async Task<ServiceResponse<List<ListLessonDto>>> GetListLessonByCourseId(int CourseId)
        {
            var response = new ServiceResponse<List<ListLessonDto>>();
            try
            {
                var lessons = await _lessonRepository.GetListLessonByCourseId(CourseId);
                response.Data = lessons.Select(l => _mapper.Map<ListLessonDto>(l)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lessons");
                response.Success = false;
                response.Message = "Error getting lessons";
                return response;
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
                    response.Message = "Lesson not found";
                    return response;
                }
                response.Data = _mapper.Map<LessonDto>(lesson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lesson");
                response.Success = false;
                response.Message = "Error getting lesson";
                return response;
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
                    response.Message = "Lesson not found";
                    return response;
                }

                // Cập nhật trực tiếp entity đã tồn tại thay vì tạo mới
                lesson.Title = dto.Title;
                lesson.Description = dto.Description;
                
                await _lessonRepository.UpdateLesson(lesson);
                response.Data = _mapper.Map<LessonDto>(lesson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lesson");
                response.Success = false;
                response.Message = "Error updating lesson";
                return response;
            }
            return response;
        }
        public async Task<ServiceResponse<bool>> DeleteLesson(DeleteLessonDto dto)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var lesson = await _lessonRepository.GetLessonById(dto.LessonId);
                if (lesson == null)
                {
                    response.Success = false;
                    response.Message = "Lesson not found";
                    response.Data = false;
                    return response;
                }
                await _lessonRepository.DeleteLesson(dto.LessonId);
                response.Data = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lesson");
                response.Success = false;
                response.Message = "Error deleting lesson";
                response.Data = false;
                return response;
            }
            return response;
        }


    }
}
