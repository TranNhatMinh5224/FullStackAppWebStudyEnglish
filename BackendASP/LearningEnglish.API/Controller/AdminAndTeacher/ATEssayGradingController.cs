using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Extensions;
using LearningEnglish.Application.Interface.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher;

[ApiController]
[Route("api/admin-teacher/essay-grading")]
[Authorize(Roles = "Admin,Teacher")]
public class ATEssayGradingController : ControllerBase
{
    private readonly IEssayGradingService _gradingService;
    private readonly ILogger<ATEssayGradingController> _logger;

    public ATEssayGradingController(
        IEssayGradingService gradingService,
        ILogger<ATEssayGradingController> logger)
    {
        _gradingService = gradingService;
        _logger = logger;
    }

    /// <summary>
    /// Grade essay submission using AI (Gemini)
    /// </summary>
    /// <param name="submissionId">ID of the essay submission</param>
    /// <returns>Grading result with score, feedback, and breakdown</returns>
    [HttpPost("ai/{submissionId}")]
    public async Task<ActionResult<ServiceResponse<EssayGradingResultDto>>> GradeWithAI(
        int submissionId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🤖 Admin/Teacher requesting AI grading for submission {SubmissionId}", submissionId);

        var result = await _gradingService.GradeEssayWithAIAsync(submissionId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Grade essay submission by teacher (override AI grading)
    /// </summary>
    /// <param name="submissionId">ID of the essay submission</param>
    /// <param name="dto">Teacher grading data (score and feedback)</param>
    /// <returns>Grading result with teacher's score and feedback</returns>
    [HttpPost("teacher/{submissionId}")]
    public async Task<ActionResult<ServiceResponse<EssayGradingResultDto>>> GradeByTeacher(
        int submissionId,
        [FromBody] TeacherGradingDto dto,
        CancellationToken cancellationToken)
    {
        var teacherId = User.GetUserId();
        
        _logger.LogInformation("👨‍🏫 Teacher {TeacherId} grading submission {SubmissionId}", teacherId, submissionId);

        var result = await _gradingService.GradeByTeacherAsync(submissionId, dto, teacherId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }
}
