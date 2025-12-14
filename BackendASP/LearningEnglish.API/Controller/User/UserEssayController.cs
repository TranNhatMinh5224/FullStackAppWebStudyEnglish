using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.User
{
    /// <summary>
    /// Controller xem Essays cho Student
    /// Student chỉ có quyền xem essays, không được tạo/sửa/xóa
    /// </summary>
    [Route("api/user/essays")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class UserEssayController : ControllerBase
    {
        private readonly IEssayService _essayService;

        public UserEssayController(IEssayService essayService)
        {
            _essayService = essayService;
        }

        /// <summary>
        /// Xem chi tiết một essay (chỉ đọc)
        /// Student chỉ xem được essays trong courses đã enroll
        /// </summary>
        [HttpGet("{essayId}")]
        public async Task<IActionResult> GetEssay(int essayId)
        {
            var result = await _essayService.GetEssayByIdAsync(essayId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy danh sách essays theo assessment ID (chỉ đọc)
        /// Student chỉ xem được essays từ assessments trong courses đã enroll
        /// </summary>
        [HttpGet("assessment/{assessmentId}")]
        public async Task<IActionResult> GetEssaysByAssessment(int assessmentId)
        {
            var result = await _essayService.GetEssaysByAssessmentIdAsync(assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
