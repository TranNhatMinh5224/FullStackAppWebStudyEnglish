using CleanDemo.Application.Common;
using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Domain.Entities;
using CleanDemo.Domain.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace CleanDemo.Application.Service
{
    public class MiniTestService : IMiniTestService
    {
        private readonly IMiniTestRepository _miniTestRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MiniTestService> _logger;

        public MiniTestService(
            IMiniTestRepository miniTestRepository,
            ILessonRepository lessonRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<MiniTestService> logger)
        {
            _miniTestRepository = miniTestRepository;
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
        }
        // Implement phương thức Admin Thêm MiniTest vào Lesson
        public async Task<ServiceResponse<MiniTestResponseDto>> AdminAddMiniTest(MiniTestDto dto)
        {
            var response = new ServiceResponse<MiniTestResponseDto>();
            try
            {

                var lesson = await _lessonRepository.GetLessonById(dto.LessonId);
                if (lesson == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Lesson không tồn tại.";
                    return response;
                }


                var course = await _courseRepository.GetCourseById(lesson.CourseId);

                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Khóa học của bài học không tồn tại.";
                    return response;
                }
                // Admin chỉ được thêm vào System course
                if (course.Type != CourseType.System)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Admin chỉ có thể thêm mini test vào bài học của khóa học hệ thống";
                    return response;
                }

                // Kiểm tra trùng tên
                var exists = await _miniTestRepository.MiniTestExistsInLesson(dto.Title, dto.LessonId);
                if (exists)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mini test đã tồn tại trong bài học này";
                    return response;
                }

                // Tạo MiniTest mới
                var miniTest = new MiniTest
                {
                    Title = dto.Title,
                    LessonId = dto.LessonId
                };

                await _miniTestRepository.AddMiniTestAsync(miniTest);

                response.StatusCode = 201;
                response.Message = "Tạo mini test thành công";
                response.Data = _mapper.Map<MiniTestResponseDto>(miniTest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding mini test for admin");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        // Implement phương thức Teacher Thêm MiniTest vào Lesson
        public async Task<ServiceResponse<MiniTestResponseDto>> TeacherAddMiniTest(MiniTestDto dto, int teacherId)
        {
            var response = new ServiceResponse<MiniTestResponseDto>();
            try
            {

                var lesson = await _lessonRepository.GetLessonById(dto.LessonId);
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


                if (course.Type != CourseType.Teacher || course.TeacherId != teacherId)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher chỉ có thể thêm mini test vào bài học của khóa học do mình tạo";
                    return response;
                }


                var exists = await _miniTestRepository.MiniTestExistsInLesson(dto.Title, dto.LessonId);
                if (exists)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mini test đã tồn tại trong bài học này";
                    return response;
                }

                var miniTest = new MiniTest
                {
                    Title = dto.Title,
                    LessonId = dto.LessonId
                };

                await _miniTestRepository.AddMiniTestAsync(miniTest);

                response.StatusCode = 201;
                response.Message = "Tạo mini test thành công";
                response.Data = _mapper.Map<MiniTestResponseDto>(miniTest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding mini test for teacher");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }
    }
}