using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Authorization;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Teacher.TeacherQuizSection
{
    [ApiController]
    [Route("api/teacher/quiz-sections")]
    [RequireTeacherRole]
    public class TeacherQuizSectionController : ControllerBase
    {
        private readonly IQuizSectionService _quizSectionService;
        private readonly IQuestionService _questionService;
        private readonly ILogger<TeacherQuizSectionController> _logger;

        public TeacherQuizSectionController(
            IQuizSectionService quizSectionService,
            IQuestionService questionService,
            ILogger<TeacherQuizSectionController> logger)
        {
            _quizSectionService = quizSectionService;
            _questionService = questionService;
            _logger = logger;
        }

        // POST: api/teacher/quiz-sections - tạo mới quiz section (own courses only)
        [HttpPost]
        public async Task<IActionResult> CreateQuizSection([FromBody] CreateQuizSectionDto createDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang tạo QuizSection mới", teacherId);

            var result = await _quizSectionService.CreateQuizSectionAsync(createDto);

            if (!result.Success)
                _logger.LogWarning("Tạo QuizSection thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Tạo QuizSection thành công với ID: {QuizSectionId}", result.Data?.QuizSectionId);

            return result.Success
                ? CreatedAtAction(nameof(GetQuizSectionById), new { id = result.Data?.QuizSectionId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/quiz-sections/{id} - lấy quiz section theo ID (own courses only)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuizSectionById(int id)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang lấy QuizSection {QuizSectionId}", teacherId, id);

            var result = await _quizSectionService.GetQuizSectionByIdAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/quiz-sections/by-quiz/{quizId} - lấy danh sách quiz sections theo quiz ID (own courses only)
        [HttpGet("by-quiz/{quizId}")]
        public async Task<IActionResult> GetQuizSectionsByQuizId(int quizId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang lấy QuizSections của Quiz {QuizId}", teacherId, quizId);

            var result = await _quizSectionService.GetQuizSectionsByQuizIdAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/teacher/quiz-sections/{id} - cập nhật quiz section (own courses only)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuizSection(int id, [FromBody] UpdateQuizSectionDto updateDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang cập nhật QuizSection {QuizSectionId}", teacherId, id);

            var result = await _quizSectionService.UpdateQuizSectionAsync(id, updateDto);

            if (!result.Success)
                _logger.LogWarning("Cập nhật QuizSection thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Cập nhật QuizSection thành công: {QuizSectionId}", id);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/teacher/quiz-sections/{id} - xóa quiz section (own courses only)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuizSection(int id)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xóa QuizSection {QuizSectionId}", teacherId, id);

            var result = await _quizSectionService.DeleteQuizSectionAsync(id);

            if (!result.Success)
                _logger.LogWarning("Xóa QuizSection thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Xóa QuizSection thành công: {QuizSectionId}", id);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/teacher/quiz-sections/bulk - Bulk tạo section với groups và questions
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateQuizSectionBulk([FromBody] QuizSectionBulkCreateDto bulkCreateDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang bulk tạo QuizSection với {GroupCount} groups", 
                teacherId, bulkCreateDto.QuizGroups?.Count ?? 0);

            var result = await _questionService.CreateQuizSectionBulkAsync(bulkCreateDto);

            if (!result.Success)
                _logger.LogWarning("Bulk tạo QuizSection thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Bulk tạo QuizSection thành công với ID: {QuizSectionId}", result.Data?.QuizSectionId);

            return result.Success
                ? StatusCode(201, result)
                : StatusCode(result.StatusCode, result);
        }
    }
}
