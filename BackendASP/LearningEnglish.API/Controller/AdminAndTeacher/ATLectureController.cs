using LearningEnglish.Application.DTOS;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [Route("api/[controller]")]
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


        // Lấy thông tin lecture theo ID với chi tiết

        [HttpGet("{lectureId}")]
        public async Task<IActionResult> GetLecture(int lectureId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _lectureService.GetLectureByIdAsync(lectureId, userId);

                if (!result.Success)
                    return NotFound(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lecture với ID: {LectureId}", lectureId);
                return StatusCode(500, "Có lỗi xảy ra khi lấy thông tin lecture");
            }
        }


        // Lấy tất cả lecture theo module ID

        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetLecturesByModule(int moduleId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _lectureService.GetLecturesByModuleIdAsync(moduleId, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lecture theo ModuleId: {ModuleId}", moduleId);
                return StatusCode(500, "Có lỗi xảy ra khi lấy danh sách lecture");
            }
        }


        // Lấy cấu trúc cây lecture theo module ID

        [HttpGet("module/{moduleId}/tree")]
        public async Task<IActionResult> GetLectureTree(int moduleId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _lectureService.GetLectureTreeByModuleIdAsync(moduleId, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy cấu trúc cây lecture theo ModuleId: {ModuleId}", moduleId);
                return StatusCode(500, "Có lỗi xảy ra khi lấy cấu trúc cây lecture");
            }
        }


        // Tạo lecture mới

        [HttpPost]
        public async Task<IActionResult> CreateLecture([FromBody] CreateLectureDto createLectureDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetCurrentUserId();
                var result = await _lectureService.CreateLectureAsync(createLectureDto, userId);

                if (!result.Success)
                    return BadRequest(result);

                return CreatedAtAction(nameof(GetLecture), new { lectureId = result.Data?.LectureId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo lecture mới: {@CreateLectureDto}", createLectureDto);
                return StatusCode(500, "Có lỗi xảy ra khi tạo lecture");
            }
        }


        // Cập nhật lecture (Admin có thể cập nhật bất kỳ, Teacher chỉ lecture của mình)
        [HttpPut("{lectureId}")]
        public async Task<IActionResult> UpdateLecture(int lectureId, [FromBody] UpdateLectureDto updateLectureDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                var result = await _lectureService.UpdateLectureWithAuthorizationAsync(lectureId, updateLectureDto, userId, userRole);

                if (!result.Success)
                {
                    if (result.Message?.Contains("không có quyền") == true)
                        return Forbid(result.Message);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật lecture với ID: {LectureId}, {@UpdateLectureDto}", lectureId, updateLectureDto);
                return StatusCode(500, "Có lỗi xảy ra khi cập nhật lecture");
            }
        }


        // Xóa lecture (Admin có thể xóa bất kỳ, Teacher chỉ lecture của mình)

        [HttpDelete("{lectureId}")]
        public async Task<IActionResult> DeleteLecture(int lectureId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                var result = await _lectureService.DeleteLectureWithAuthorizationAsync(lectureId, userId, userRole);

                if (!result.Success)
                {
                    if (result.Message?.Contains("không có quyền") == true)
                        return Forbid(result.Message);
                    if (result.Message?.Contains("không tìm thấy") == true)
                        return NotFound(result);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa lecture với ID: {LectureId}", lectureId);
                return StatusCode(500, "Có lỗi xảy ra khi xóa lecture");
            }
        }


        // Sắp xếp lại thứ tự lecture

        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderLectures([FromBody] List<ReorderLectureDto> reorderDtos)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                var result = await _lectureService.ReorderLecturesAsync(reorderDtos, userId, userRole);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi sắp xếp lại lecture: {@ReorderDtos}", reorderDtos);
                return StatusCode(500, "Có lỗi xảy ra khi sắp xếp lại lecture");
            }
        }
    }
}
