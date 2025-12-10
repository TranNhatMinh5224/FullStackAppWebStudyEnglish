using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/quizzes")]
    [Authorize(Roles = "Student")]
    public class UserQuizController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public UserQuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        // GET: api/User/Quizz/{assessmentId} - Get all quizzes by assessment ID (for students)
        [HttpGet("Quizz/{assessmentId}")]
        public async Task<IActionResult> GetQuizInformation(int assessmentId)
        {
            var result = await _quizService.GetQuizzesByAssessmentIdAsync(assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/User/quiz/{quizId} - Get quiz by ID (for students)
        [HttpGet("quiz/{quizId}")]
        public async Task<IActionResult> GetQuizById(int quizId)
        {
            var result = await _quizService.GetQuizByIdAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}