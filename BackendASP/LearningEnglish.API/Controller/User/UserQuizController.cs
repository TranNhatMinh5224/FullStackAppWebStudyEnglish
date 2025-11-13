using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/User/Quiz")]
    [Authorize(Roles = "Student")]
    public class UserQuizController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public UserQuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        // Lấy thông tin đầy đủ của Quiz theo ID (dành cho học sinh)
        [HttpGet("Quizz/{AssessmentId}")]
        public async Task<IActionResult> GetFullQuiz(int assessmentId)
        {
            var result = await _quizService.GetQuizzesByAssessmentIdAsync(assessmentId);

            if (result.Success)
            {
                return Ok(result);
            }


            return NotFound(result);
        }
        [HttpGet("quiz/{quizId}")]
        public async Task<IActionResult> GetQuizById(int quizId)
        {
            var result = await _quizService.GetQuizByIdAsync(quizId);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        
    }
}