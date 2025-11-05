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
        // Implement phương thức Lấy tất cả MiniTest
        public async Task<ServiceResponse<List<MiniTestResponseDto>>> GetAllMiniTests(int lessonId)
        {
            var response = new ServiceResponse<List<MiniTestResponseDto>>();
            try
            {
                var miniTests = await _miniTestRepository.GetListMiniTestByIdLesson(lessonId);
                response.Data = _mapper.Map<List<MiniTestResponseDto>>(miniTests);
                response.StatusCode = 200;
                response.Message = "Lấy tất cả mini test thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "lỗi lấy tất cả mini test");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }
        // Implement phương thức Admin Cập nhật MiniTest
        public async Task<ServiceResponse<MiniTestResponseDto>> AdminUpdateMiniTest(int miniTestId, UpdateMiniTestDto dto)
        {
            var response = new ServiceResponse<MiniTestResponseDto>();
            try
            {
                var existingMiniTest = await _miniTestRepository.GetMiniTestByIdAsync(miniTestId);
                if (existingMiniTest == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy mini test";
                    return response;
                }

                if (existingMiniTest.Lesson?.Course?.Type != CourseType.System)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Admin chỉ có thể cập nhật mini test thuộc bài học của khóa học hệ thống";
                    return response;
                }

                if (dto.Title != existingMiniTest.Title)
                {
                    var titleExists = await _miniTestRepository.MiniTestExistsInLesson(dto.Title, existingMiniTest.LessonId);
                    if (titleExists)
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Tên mini test đã tồn tại trong bài học này";
                        return response;
                    }
                }

                existingMiniTest.Title = dto.Title;
                await _miniTestRepository.UpdateMiniTestAsync(existingMiniTest);

                response.StatusCode = 200;
                response.Message = "Cập nhật mini test thành công";
                response.Data = _mapper.Map<MiniTestResponseDto>(existingMiniTest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating mini test for admin");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }
        // Implement phương thức Teacher Cập nhật MiniTest
        public async Task<ServiceResponse<MiniTestResponseDto>> TeacherUpdateMiniTest(int miniTestId, UpdateMiniTestDto dto, int teacherId)
        {
            var response = new ServiceResponse<MiniTestResponseDto>();
            try
            {
                var existingMiniTest = await _miniTestRepository.GetMiniTestByIdAsync(miniTestId);
                if (existingMiniTest == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy mini test";
                    return response;
                }

                if (existingMiniTest.Lesson?.Course?.Type != CourseType.Teacher ||
                    existingMiniTest.Lesson?.Course?.TeacherId != teacherId)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher chỉ có thể cập nhật mini test của bài học thuộc khóa học do mình tạo";
                    return response;
                }

                if (dto.Title != existingMiniTest.Title)
                {
                    var titleExists = await _miniTestRepository.MiniTestExistsInLesson(dto.Title, existingMiniTest.LessonId);
                    if (titleExists)
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Tên mini test đã tồn tại trong bài học này";
                        return response;
                    }
                }

                existingMiniTest.Title = dto.Title;
                await _miniTestRepository.UpdateMiniTestAsync(existingMiniTest);

                response.StatusCode = 200;
                response.Message = "Cập nhật mini test thành công";
                response.Data = _mapper.Map<MiniTestResponseDto>(existingMiniTest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating mini test for teacher");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

    }
}