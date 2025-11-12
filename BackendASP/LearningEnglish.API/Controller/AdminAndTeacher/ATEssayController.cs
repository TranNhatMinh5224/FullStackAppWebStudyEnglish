using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [Route("api/Essay")]
    [ApiController]
    [Authorize(Roles = "Admin, Teacher")]
    public class ATEssayController : ControllerBase
    {
        private readonly IEssayService _essayService;

        public ATEssayController(IEssayService essayService)
        {
            _essayService = essayService;
        }

        
        //Controller lấy thông tin Essay theo ID
        
        [HttpGet("{essayId}")]
        public async Task<IActionResult> GetEssay(int essayId)
        {
            var result = await _essayService.GetEssayByIdAsync(essayId);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }

        //Controller lấy danh sách Essay theo Assessment ID
        [HttpGet("assessment/{assessmentId}")]
        public async Task<IActionResult> GetEssaysByAssessment(int assessmentId)
        {
            var result = await _essayService.GetEssaysByAssessmentIdAsync(assessmentId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        //Controller tạo Essay mới
        [HttpPost("create")]
        public async Task<IActionResult> CreateEssay([FromBody] CreateEssayDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Lấy thông tin Teacher từ token (nếu không phải Admin)
            int? teacherId = null;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Teacher")
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int parsedUserId))
                {
                    teacherId = parsedUserId;
                }
            }

            var result = await _essayService.CreateEssayAsync(createDto, teacherId);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetEssay), new { essayId = result.Data?.EssayId }, result);
            }

            return BadRequest(result);
        }

        //Controller cập nhật Essay
        [HttpPut("update/{essayId}")]
        public async Task<IActionResult> UpdateEssay(int essayId, [FromBody] UpdateEssayDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Lấy thông tin Teacher từ token (nếu không phải Admin)
            int? teacherId = null;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Teacher")
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int parsedUserId))
                {
                    teacherId = parsedUserId;
                }
            }

            var result = await _essayService.UpdateEssayAsync(essayId, updateDto, teacherId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


        //controller Xóa Essay theo id

        [HttpDelete("delete/{essayId}")]
        public async Task<IActionResult> DeleteEssay(int essayId)
        {
            // Lấy thông tin Teacher từ token (nếu không phải Admin)
            int? teacherId = null;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Teacher")
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int parsedUserId))
                {
                    teacherId = parsedUserId;
                }
            }

            var result = await _essayService.DeleteEssayAsync(essayId, teacherId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}