using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [Route("api/lectures")]
    [ApiController]
    [Authorize]
    public class ATLectureController : ControllerBase
    {
        private readonly ILectureService _lectureService;
        private readonly ILogger<ATLectureController> _logger;

        public ATLectureController(ILectureService lectureService, ILogger<ATLectureController> logger)
        {
            _lectureService = lectureService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        // GET: api/lectures/{lectureId} - lấy lecture theo ID
        [HttpGet("{lectureId}")]
        public async Task<IActionResult> GetLecture(int lectureId)
        {
            var userId = GetCurrentUserId();
            var result = await _lectureService.GetLectureByIdAsync(lectureId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/lectures/module/{moduleId} - lấy tất cả lecture theo module ID
        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetLecturesByModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _lectureService.GetLecturesByModuleIdAsync(moduleId, userId);
            return Ok(result);
        }

        // GET: api/atlecture/module/{moduleId}/tree - lấy cây lecture theo module ID
        [HttpGet("module/{moduleId}/tree")]
        public async Task<IActionResult> GetLectureTree(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _lectureService.GetLectureTreeByModuleIdAsync(moduleId, userId);
            return Ok(result);
        }

        // POST: api/atlecture - tạo mới lecture (Admin/Teacher)
        [HttpPost]
        public async Task<IActionResult> CreateLecture([FromBody] CreateLectureDto createLectureDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var result = await _lectureService.CreateLectureAsync(createLectureDto, userId);
            return result.Success
                ? CreatedAtAction(nameof(GetLecture), new { lectureId = result.Data?.LectureId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // POST: api/atlecture/bulk - tạo thêm nhiều lecture cùng lúc (Admin/Teacher)
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreateLectures([FromBody] BulkCreateLecturesDto bulkCreateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var result = await _lectureService.BulkCreateLecturesAsync(bulkCreateDto, userId);

            return result.Success
                ? Ok(result)
                : StatusCode(result.StatusCode, result);
        }

        // PUT: api/atlecture/{lectureId} - sửa lecture, teacher chỉ sửa của riêng teacher, còn admin sửa tất cả
        [HttpPut("{lectureId}")]
        public async Task<IActionResult> UpdateLecture(int lectureId, [FromBody] UpdateLectureDto updateLectureDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _lectureService.UpdateLectureWithAuthorizationAsync(lectureId, updateLectureDto, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/atlecture/{lectureId} - xoá lecture, teacher chỉ xoá của riêng teacher, còn admin xoá tất cả
        [HttpDelete("{lectureId}")]
        public async Task<IActionResult> DeleteLecture(int lectureId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _lectureService.DeleteLectureWithAuthorizationAsync(lectureId, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/atlecture/reorder - sắp xếp lại thứ tự các lecture trong cùng một module
        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderLectures([FromBody] List<ReorderLectureDto> reorderDtos)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _lectureService.ReorderLecturesAsync(reorderDtos, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
