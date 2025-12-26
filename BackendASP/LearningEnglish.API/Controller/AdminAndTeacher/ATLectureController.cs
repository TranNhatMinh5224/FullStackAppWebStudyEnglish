using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [Route("api/lectures")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin, Teacher")]
    public class ATLectureController : ControllerBase
    {
        private readonly ILectureService _lectureService;
        private readonly ILogger<ATLectureController> _logger;

        public ATLectureController(ILectureService lectureService, ILogger<ATLectureController> logger)
        {
            _lectureService = lectureService;
            _logger = logger;
        }


        // GET: api/lectures/{lectureId} - lấy lecture theo ID
        [HttpGet("{lectureId}")]
        public async Task<IActionResult> GetLecture(int lectureId)
        {
            var userId = User.GetUserId();
            var result = await _lectureService.GetLectureByIdAsync(lectureId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/lectures/module/{moduleId} - lấy tất cả lecture theo module ID
        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetLecturesByModule(int moduleId)
        {
            var userId = User.GetUserId();
            var result = await _lectureService.GetLecturesByModuleIdAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/atlecture/module/{moduleId}/tree - lấy cây lecture theo module ID
        [HttpGet("module/{moduleId}/tree")]
        public async Task<IActionResult> GetLectureTree(int moduleId)
        {
            var userId = User.GetUserId();
            var result = await _lectureService.GetLectureTreeByModuleIdAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/atlecture - tạo mới lecture (Admin/Teacher)
        // Admin: Cần permission Admin.Lesson.Manage (chỉ SuperAdmin, ContentAdmin có)
        // Teacher: Chỉ tạo lecture cho modules của own courses
        [HttpPost]
        [RequirePermission("Admin.Lesson.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateLecture([FromBody] CreateLectureDto createLectureDto)
        {
            var userId = User.GetUserId();
            var result = await _lectureService.CreateLectureAsync(createLectureDto, userId);
            return result.Success
                ? CreatedAtAction(nameof(GetLecture), new { lectureId = result.Data?.LectureId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // POST: api/atlecture/bulk - tạo thêm nhiều lecture cùng lúc (Admin/Teacher)
        // Admin: Cần permission Admin.Lesson.Manage
        // Teacher: Chỉ tạo lecture cho modules của own courses
        [HttpPost("bulk")]
        [RequirePermission("Admin.Lesson.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> BulkCreateLectures([FromBody] BulkCreateLecturesDto bulkCreateDto)
        {
            var userId = User.GetUserId();
            var result = await _lectureService.BulkCreateLecturesAsync(bulkCreateDto, userId);

            return result.Success
                ? Ok(result)
                : StatusCode(result.StatusCode, result);
        }

        // PUT: api/atlecture/{lectureId} - sửa lecture
        // Admin: Cần permission Admin.Lesson.Manage
        // Teacher: Chỉ sửa lecture của own courses (RLS check)
        [HttpPut("{lectureId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateLecture(int lectureId, [FromBody] UpdateLectureDto updateLectureDto)
        {
            var userId = User.GetUserId();
            var userRole = User.GetPrimaryRole();
            var result = await _lectureService.UpdateLectureWithAuthorizationAsync(lectureId, updateLectureDto, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/atlecture/{lectureId} - xoá lecture
        // Admin: Cần permission Admin.Lesson.Manage
        // Teacher: Chỉ xóa lecture của own courses (RLS check)
        [HttpDelete("{lectureId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteLecture(int lectureId)
        {
            var userId = User.GetUserId();
            var userRole = User.GetPrimaryRole();
            var result = await _lectureService.DeleteLectureWithAuthorizationAsync(lectureId, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/atlecture/reorder - sắp xếp lại thứ tự các lecture trong cùng một module
        // Admin: Cần permission Admin.Lesson.Manage
        // Teacher: Chỉ reorder lecture của own courses (RLS check)
        [HttpPost("reorder")]
        [RequirePermission("Admin.Lesson.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ReorderLectures([FromBody] List<ReorderLectureDto> reorderDtos)
        {
            var userId = User.GetUserId();
            var userRole = User.GetPrimaryRole();
            var result = await _lectureService.ReorderLecturesAsync(reorderDtos, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
