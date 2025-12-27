using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Admin
{
    [Route("api/admin/lectures")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]
    public class AdminLectureController : ControllerBase
    {
        private readonly ILectureService _lectureService;
        private readonly ILogger<AdminLectureController> _logger;

        public AdminLectureController(ILectureService lectureService, ILogger<AdminLectureController> logger)
        {
            _lectureService = lectureService;
            _logger = logger;
        }

        // POST: api/admin/lectures - Admin tạo lecture
        // RLS: lectures_policy_admin_all (Admin có quyền tạo lecture trong bất kỳ module nào)
        [HttpPost]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> CreateLecture([FromBody] CreateLectureDto createLectureDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang tạo lecture cho module {ModuleId}", adminId, createLectureDto.ModuleId);

            var result = await _lectureService.AdminCreateLecture(createLectureDto);
            return result.Success
                ? CreatedAtAction(nameof(GetLecture), new { lectureId = result.Data?.LectureId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // POST: api/admin/lectures/bulk - Admin tạo nhiều lectures cùng lúc
        // RLS: lectures_policy_admin_all (Admin có quyền tạo lecture trong bất kỳ module nào)
        [HttpPost("bulk")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> BulkCreateLectures([FromBody] BulkCreateLecturesDto bulkCreateDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang bulk create lectures cho module {ModuleId}", adminId, bulkCreateDto.ModuleId);

            var result = await _lectureService.AdminBulkCreateLectures(bulkCreateDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/lectures/{lectureId} - Admin xem chi tiết lecture
        // RLS: lectures_policy_admin_all (Admin có quyền xem tất cả lectures)
        [HttpGet("{lectureId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> GetLecture(int lectureId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem lecture {LectureId}", adminId, lectureId);

            // RLS đã filter: Admin có quyền xem tất cả lectures
            var result = await _lectureService.GetLectureByIdAsync(lectureId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/lectures/module/{moduleId} - Admin xem danh sách lectures theo module
        // RLS: lectures_policy_admin_all (Admin có quyền xem tất cả lectures)
        [HttpGet("module/{moduleId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> GetLecturesByModule(int moduleId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem danh sách lectures của module {ModuleId}", adminId, moduleId);

            // RLS đã filter: Admin có quyền xem tất cả lectures
            // userId = null vì Admin không cần progress info
            var result = await _lectureService.GetLecturesByModuleIdAsync(moduleId, null);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/lectures/module/{moduleId}/tree - Admin xem cây lecture theo module
        // RLS: lectures_policy_admin_all (Admin có quyền xem tất cả lectures)
        [HttpGet("module/{moduleId}/tree")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> GetLectureTree(int moduleId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem cây lecture của module {ModuleId}", adminId, moduleId);

            // RLS đã filter: Admin có quyền xem tất cả lectures
            // userId = null vì Admin không cần progress info
            var result = await _lectureService.GetLectureTreeByModuleIdAsync(moduleId, null);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/admin/lectures/{lectureId} - Admin cập nhật lecture
        // RLS: lectures_policy_admin_all (Admin có quyền cập nhật tất cả lectures)
        [HttpPut("{lectureId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> UpdateLecture(int lectureId, [FromBody] UpdateLectureDto updateLectureDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang cập nhật lecture {LectureId}", adminId, lectureId);

            // RLS đã filter: Admin có quyền cập nhật tất cả lectures
            var result = await _lectureService.UpdateLecture(lectureId, updateLectureDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/admin/lectures/{lectureId} - Admin xóa lecture
        // RLS: lectures_policy_admin_all (Admin có quyền xóa tất cả lectures)
        [HttpDelete("{lectureId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> DeleteLecture(int lectureId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xóa lecture {LectureId}", adminId, lectureId);

            // RLS đã filter: Admin có quyền xóa tất cả lectures
            var result = await _lectureService.DeleteLecture(lectureId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/admin/lectures/reorder - Admin sắp xếp lại thứ tự lectures
        // RLS: lectures_policy_admin_all (Admin có quyền reorder tất cả lectures)
        [HttpPost("reorder")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> ReorderLectures([FromBody] List<ReorderLectureDto> reorderDtos)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang reorder lectures", adminId);

            // RLS đã filter: Admin có quyền reorder tất cả lectures
            var result = await _lectureService.ReorderLectures(reorderDtos);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

