using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/quizzes")]
    [Authorize(Roles = "Student")]
    public class UserQuizController : ControllerBase
    {
        private readonly IUserQuizService _quizService;

        public UserQuizController(IUserQuizService quizService)
        {
            _quizService = quizService;
        }

        // GET: api/User/Quizz/{assessmentId} - lấy thông tin quiz theo assessment ID
        [HttpGet("Quizz/{assessmentId}")]
        public async Task<IActionResult> GetQuizInformation(int assessmentId)
        {
            var userId = User.GetUserId();
            var result = await _quizService.GetQuizzesByAssessmentIdAsync(assessmentId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/User/quiz/{quizId} - lấy quiz theo ID (cho học sinh)
        [HttpGet("quiz/{quizId}")]
        public async Task<IActionResult> GetQuizById(int quizId)
        {
            var userId = User.GetUserId();
            var result = await _quizService.GetQuizByIdAsync(quizId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}