using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Teacher.TeacherQuiz
{
    [Route("api/teacher/quizzes")]
    [ApiController]
    [RequireTeacherRole]
    public class TeacherQuizController : ControllerBase
    {
        private readonly ITeacherQuizService _quizService;
        private readonly ILogger<TeacherQuizController> _logger;

        public TeacherQuizController(ITeacherQuizService quizService, ILogger<TeacherQuizController> logger)
        {
            _quizService = quizService;
            _logger = logger;
        }

        // POST: api/teacher/quizzes - tạo mới quiz (chỉ cho assessments của own courses)
        [HttpPost]
        public async Task<IActionResult> CreateQuiz([FromBody] QuizCreateDto quizCreate)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang tạo Quiz mới", teacherId);

            var result = await _quizService.CreateQuizAsync(quizCreate, teacherId);

            if (!result.Success)
                _logger.LogWarning("Tạo Quiz thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Tạo Quiz thành công với ID: {QuizId}", result.Data?.QuizId);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/quizzes/{quizId} - lấy quiz theo ID (chỉ own courses)
        [HttpGet("{quizId}")]
        public async Task<IActionResult> GetQuiz(int quizId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang lấy Quiz {QuizId}", teacherId, quizId);

            var result = await _quizService.GetQuizByIdAsync(quizId, teacherId);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/quizzes/assessment/{assessmentId} - lấy tất cả quiz theo assessment ID (chỉ own courses)
        [HttpGet("assessment/{assessmentId}")]
        public async Task<IActionResult> GetAllQuizzInAssessment(int assessmentId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang lấy danh sách Quiz cho Assessment {AssessmentId}", teacherId, assessmentId);

            var result = await _quizService.GetQuizzesByAssessmentIdAsync(assessmentId, teacherId);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/teacher/quizzes/{quizId} - cập nhật quiz (chỉ own courses)
        [HttpPut("{quizId}")]
        public async Task<IActionResult> UpdateQuiz(int quizId, [FromBody] QuizUpdateDto quizUpdate)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang cập nhật Quiz {QuizId}", teacherId, quizId);

            var result = await _quizService.UpdateQuizAsync(quizId, quizUpdate, teacherId);

            if (!result.Success)
                _logger.LogWarning("Cập nhật Quiz thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Cập nhật Quiz thành công: {QuizId}", quizId);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/teacher/quizzes/{quizId} - xóa quiz (chỉ own courses)
        [HttpDelete("{quizId}")]
        public async Task<IActionResult> DeleteQuiz(int quizId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xóa Quiz {QuizId}", teacherId, quizId);

            var result = await _quizService.DeleteQuizAsync(quizId, teacherId);

            if (!result.Success)
                _logger.LogWarning("Xóa Quiz thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Xóa Quiz thành công: {QuizId}", quizId);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
