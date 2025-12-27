using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Teacher
{
    [Route("api/teacher/lectures")]
    [ApiController]
    [RequireTeacherRole]
    public class TeacherLectureController : ControllerBase
    {
        private readonly ILectureService _lectureService;
        private readonly ILogger<TeacherLectureController> _logger;

        public TeacherLectureController(ILectureService lectureService, ILogger<TeacherLectureController> logger)
        {
            _lectureService = lectureService;
            _logger = logger;
        }

        // POST: api/teacher/lectures - Teacher tạo lecture
        // RLS: lectures_policy_teacher_all_own (Teacher chỉ tạo lecture trong modules của own courses)
        [HttpPost]
        public async Task<IActionResult> CreateLecture([FromBody] CreateLectureDto createLectureDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang tạo lecture cho module {ModuleId}", teacherId, createLectureDto.ModuleId);

            var result = await _lectureService.TeacherCreateLecture(createLectureDto, teacherId);
            return result.Success
                ? CreatedAtAction(nameof(GetLecture), new { lectureId = result.Data?.LectureId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // POST: api/teacher/lectures/bulk - Teacher tạo nhiều lectures cùng lúc
        // RLS: lectures_policy_teacher_all_own (Teacher chỉ tạo lecture trong modules của own courses)
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreateLectures([FromBody] BulkCreateLecturesDto bulkCreateDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang bulk create lectures cho module {ModuleId}", teacherId, bulkCreateDto.ModuleId);

            var result = await _lectureService.TeacherBulkCreateLectures(bulkCreateDto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/lectures/{lectureId} - Teacher xem chi tiết lecture
        // RLS: lectures_policy_teacher_all_own (Teacher chỉ xem lectures của own courses)
        [HttpGet("{lectureId}")]
        public async Task<IActionResult> GetLecture(int lectureId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem lecture {LectureId}", teacherId, lectureId);

            // RLS đã filter: Teacher chỉ xem được lectures của own courses
            // Nếu lecture không thuộc own course → RLS sẽ filter → lecture == null → service trả về 404
            var result = await _lectureService.GetLectureByIdAsync(lectureId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/lectures/module/{moduleId} - Teacher xem danh sách lectures theo module
        // RLS: lectures_policy_teacher_all_own (Teacher chỉ xem lectures của own courses)
        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetLecturesByModule(int moduleId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem danh sách lectures của module {ModuleId}", teacherId, moduleId);

            // RLS đã filter: Teacher chỉ xem được lectures của modules thuộc own courses
            // Nếu module không thuộc own course → RLS sẽ filter → lectures = empty list
            // userId = null vì Teacher không cần progress info
            var result = await _lectureService.GetLecturesByModuleIdAsync(moduleId, null);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/lectures/module/{moduleId}/tree - Teacher xem cây lecture theo module
        // RLS: lectures_policy_teacher_all_own (Teacher chỉ xem lectures của own courses)
        [HttpGet("module/{moduleId}/tree")]
        public async Task<IActionResult> GetLectureTree(int moduleId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem cây lecture của module {ModuleId}", teacherId, moduleId);

            // RLS đã filter: Teacher chỉ xem được lectures của modules thuộc own courses
            // Nếu module không thuộc own course → RLS sẽ filter → lectures = empty list
            // userId = null vì Teacher không cần progress info
            var result = await _lectureService.GetLectureTreeByModuleIdAsync(moduleId, null);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/teacher/lectures/{lectureId} - Teacher cập nhật lecture
        // RLS: lectures_policy_teacher_all_own (Teacher chỉ cập nhật lectures của own courses)
        [HttpPut("{lectureId}")]
        public async Task<IActionResult> UpdateLecture(int lectureId, [FromBody] UpdateLectureDto updateLectureDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang cập nhật lecture {LectureId}", teacherId, lectureId);

            // RLS đã filter: Teacher chỉ cập nhật được lectures của own courses
            // Nếu lecture không thuộc own course → RLS sẽ filter → lecture == null → service trả về 404
            var result = await _lectureService.UpdateLecture(lectureId, updateLectureDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/teacher/lectures/{lectureId} - Teacher xóa lecture
        // RLS: lectures_policy_teacher_all_own (Teacher chỉ xóa lectures của own courses)
        [HttpDelete("{lectureId}")]
        public async Task<IActionResult> DeleteLecture(int lectureId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xóa lecture {LectureId}", teacherId, lectureId);

            // RLS đã filter: Teacher chỉ xóa được lectures của own courses
            // Nếu lecture không thuộc own course → RLS sẽ filter → lecture == null → service trả về 404
            var result = await _lectureService.DeleteLecture(lectureId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/teacher/lectures/reorder - Teacher sắp xếp lại thứ tự lectures
        // RLS: lectures_policy_teacher_all_own (Teacher chỉ reorder lectures của own courses)
        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderLectures([FromBody] List<ReorderLectureDto> reorderDtos)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang reorder lectures", teacherId);

            // RLS đã filter: Teacher chỉ reorder được lectures của own courses
            // Nếu lecture không thuộc own course → RLS sẽ filter → lecture == null → skip
            var result = await _lectureService.ReorderLectures(reorderDtos);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

