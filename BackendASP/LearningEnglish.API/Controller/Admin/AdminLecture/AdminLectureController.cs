using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.Services.Lecture;
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
        private readonly IAdminLectureService _lectureService;
        private readonly ILogger<AdminLectureController> _logger;

        public AdminLectureController(IAdminLectureService lectureService, ILogger<AdminLectureController> logger)
        {
            _lectureService = lectureService;
            _logger = logger;
        }

        // POST: api/admin/lectures - Admin tạo lecture
       
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
      
        [HttpGet("{lectureId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> GetLecture(int lectureId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem lecture {LectureId}", adminId, lectureId);

          
            var result = await _lectureService.GetLectureByIdAsync(lectureId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/lectures/module/{moduleId} - Admin xem danh sách lectures theo module
      
        [HttpGet("module/{moduleId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> GetLecturesByModule(int moduleId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem danh sách lectures của module {ModuleId}", adminId, moduleId);

          
            var result = await _lectureService.GetLecturesByModuleIdAsync(moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/lectures/module/{moduleId}/tree - Admin xem cây lecture theo module
      
        [HttpGet("module/{moduleId}/tree")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> GetLectureTree(int moduleId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem cây lecture của module {ModuleId}", adminId, moduleId);

        
            var result = await _lectureService.GetLectureTreeByModuleIdAsync(moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/admin/lectures/{lectureId} - Admin cập nhật lecture
        
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> UpdateLecture(int lectureId, [FromBody] UpdateLectureDto updateLectureDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang cập nhật lecture {LectureId}", adminId, lectureId);

          
            var result = await _lectureService.UpdateLecture(lectureId, updateLectureDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/admin/lectures/{lectureId} - Admin xóa lecture
       
        [HttpDelete("{lectureId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> DeleteLecture(int lectureId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xóa lecture {LectureId}", adminId, lectureId);

           
            var result = await _lectureService.DeleteLecture(lectureId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/admin/lectures/reorder - Admin sắp xếp lại thứ tự lectures
       
        [HttpPost("reorder")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> ReorderLectures([FromBody] List<ReorderLectureDto> reorderDtos)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang reorder lectures", adminId);

           
            var result = await _lectureService.ReorderLectures(reorderDtos);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

