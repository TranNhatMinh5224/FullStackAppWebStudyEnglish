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

        public LessonService(ILessonRepository lessonRepository, IMapper mapper, ILogger<LessonService> logger, ICourseRepository courseRepository)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _logger = logger;
            _courseRepository = courseRepository;
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
                var TestCourse = await _courseRepository.GetCourseById(dto.CourseId);
                if (TestCourse == null || TestCourse.Type == Domain.Enums.CourseType.System)
                {
                    response.Success = false;
                    response.Message = "Cannot add lesson to this course type";
                    return response;
                }
                // check tên Lesson đã tồn  tại trong Course chưa
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
        // teacher thêm course 
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
                var TestCourse = await _courseRepository.GetCourseById(dto.CourseId);
                if (TestCourse == null || TestCourse.Type == CourseType.System)
                {
                    response.Success = false;
                    response.Message = "Cannot add lesson to this course type";
                    return response;
                }
                // check tên Lesson đã tồn  tại trong Course chưa
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
        public async Task<ServiceResponse<LessonDto>> UpdateLesson(UpdateLessonDto dto)
        {
            var response = new ServiceResponse<LessonDto>();
            try
            {
                var lesson = await _lessonRepository.GetLessonById(dto.LessonId);
                if (lesson == null)
                {
                    response.Success = false;
                    response.Message = "Lesson not found";
                    return response;
                }
                lesson.Title = dto.Title;
                lesson.Description = dto.Description;
                lesson.CourseId = dto.CourseId;
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
