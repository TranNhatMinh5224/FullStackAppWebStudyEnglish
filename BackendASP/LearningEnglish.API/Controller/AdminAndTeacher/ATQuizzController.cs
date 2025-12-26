using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [Route("api/quizzes")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin, Teacher")]
    public class ATQuizzController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public ATQuizzController(IQuizService quizService)
        {
            _quizService = quizService;
        }


        // GET: api/Quiz/ATQuizz/{quizId} - lấy quiz theo ID
        [HttpGet("{quizId}")]
        public async Task<IActionResult> GetQuiz(int quizId)
        {
            var result = await _quizService.GetQuizByIdAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/Quiz/ATQuizz/create - tạo mới quiz
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ tạo quiz cho assessments của own courses
        [HttpPost("create")]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateQuiz([FromBody] QuizCreateDto quizCreate)
        {
            var userRole = User.GetPrimaryRole();
            int? teacherId = userRole == "Teacher" ? User.GetUserId() : null;

            var result = await _quizService.CreateQuizAsync(quizCreate, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/Quiz/ATQuizz/update/{quizId} - sửa quiz
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ sửa quiz của own courses (RLS check)
        [HttpPut("update/{quizId}")]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateQuiz(int quizId, [FromBody] QuizUpdateDto quizUpdate)
        {
            var userRole = User.GetPrimaryRole();
            int? teacherId = userRole == "Teacher" ? User.GetUserId() : null;

            var result = await _quizService.UpdateQuizAsync(quizId, quizUpdate, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/Quiz/ATQuizz/delete/{quizId} - xoá quiz
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ xóa quiz của own courses (RLS check)
        [HttpDelete("delete/{quizId}")]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteQuiz(int quizId)
        {
            var userRole = User.GetPrimaryRole();
            int? teacherId = userRole == "Teacher" ? User.GetUserId() : null;

            var result = await _quizService.DeleteQuizAsync(quizId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/Quiz/ATQuizz/all/{assessmentId} - lấy tat cả quiz theo assessment ID
        [HttpGet("all/{assessmentId}")]
        public async Task<IActionResult> GetAllQuizzInAssessment(int assessmentId)
        {
            var result = await _quizService.GetQuizzesByAssessmentIdAsync(assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}