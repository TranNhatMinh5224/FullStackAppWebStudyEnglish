using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [Route("api/quizzes")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Admin, Teacher")]
    public class ATQuizzController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public ATQuizzController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.GetPrimaryRole();
        }

        // GET: api/Quiz/ATQuizz/{quizId} - lấy quiz theo ID
        [HttpGet("{quizId}")]
        public async Task<IActionResult> GetQuiz(int quizId)
        {
            var result = await _quizService.GetQuizByIdAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/Quiz/ATQuizz/create - tạo mới quiz (Admin and Teacher)
        [HttpPost("create")]
        public async Task<IActionResult> CreateQuiz([FromBody] QuizCreateDto quizCreate)
        {
            var userRole = GetCurrentUserRole();
            int? teacherId = userRole == "Teacher" ? GetCurrentUserId() : null;

            var result = await _quizService.CreateQuizAsync(quizCreate, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/Quiz/ATQuizz/update/{quizId} -sửa quiz (Admin and Teacher)
        [HttpPut("update/{quizId}")]
        public async Task<IActionResult> UpdateQuiz(int quizId, [FromBody] QuizUpdateDto quizUpdate)
        {
            var userRole = GetCurrentUserRole();
            int? teacherId = userRole == "Teacher" ? GetCurrentUserId() : null;

            var result = await _quizService.UpdateQuizAsync(quizId, quizUpdate, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/Quiz/ATQuizz/delete/{quizId} - xoá quiz (Admin and Teacher)
        [HttpDelete("delete/{quizId}")]
        public async Task<IActionResult> DeleteQuiz(int quizId)
        {
            var userRole = GetCurrentUserRole();
            int? teacherId = userRole == "Teacher" ? GetCurrentUserId() : null;

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