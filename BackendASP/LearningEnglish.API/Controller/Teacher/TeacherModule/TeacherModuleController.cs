using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Teacher
{
    [Route("api/teacher/modules")]
    [ApiController]
    [RequireTeacherRole]
    public class TeacherModuleController : ControllerBase
    {
        private readonly IModuleService _moduleService;
        private readonly ILogger<TeacherModuleController> _logger;

        public TeacherModuleController(IModuleService moduleService, ILogger<TeacherModuleController> logger)
        {
            _moduleService = moduleService;
            _logger = logger;
        }

        // POST: api/teacher/modules - Teacher tạo module
        // RLS: modules_policy_teacher_all_own (Teacher chỉ tạo module trong lessons của own courses)
        [HttpPost]
        public async Task<IActionResult> CreateModule([FromBody] CreateModuleDto createModuleDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang tạo module cho lesson {LessonId}", teacherId, createModuleDto.LessonId);

            var result = await _moduleService.TeacherCreateModule(createModuleDto, teacherId);
            return result.Success
                ? CreatedAtAction(nameof(GetModule), new { moduleId = result.Data!.ModuleId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/modules/{moduleId} - Teacher xem chi tiết module
        // RLS: modules_policy_teacher_all_own (Teacher chỉ xem modules của own courses)
        [HttpGet("{moduleId}")]
        public async Task<IActionResult> GetModule(int moduleId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem module {ModuleId}", teacherId, moduleId);

            // RLS đã filter: Teacher chỉ xem được modules của own courses
            // Nếu module không thuộc own course → RLS sẽ filter → module == null → service trả về 404
            var result = await _moduleService.GetModuleByIdAsync(moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/modules/lesson/{lessonId} - Teacher xem danh sách modules theo lesson
        // RLS: modules_policy_teacher_all_own (Teacher chỉ xem modules của own courses)
        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetModulesByLesson(int lessonId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem danh sách modules của lesson {LessonId}", teacherId, lessonId);

            // RLS đã filter: Teacher chỉ xem được modules của lessons thuộc own courses
            // Nếu lesson không thuộc own course → RLS sẽ filter → modules = empty list
            // userId = null vì Teacher không cần progress info
            var result = await _moduleService.GetModulesByLessonIdAsync(lessonId, null);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/teacher/modules/{moduleId} - Teacher cập nhật module
        // RLS: modules_policy_teacher_all_own (Teacher chỉ cập nhật modules của own courses)
        [HttpPut("{moduleId}")]
        public async Task<IActionResult> UpdateModule(int moduleId, [FromBody] UpdateModuleDto updateModuleDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang cập nhật module {ModuleId}", teacherId, moduleId);

            // RLS đã filter: Teacher chỉ cập nhật được modules của own courses
            // Nếu module không thuộc own course → RLS sẽ filter → module == null → service trả về 404
            var result = await _moduleService.UpdateModule(moduleId, updateModuleDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/teacher/modules/{moduleId} - Teacher xóa module
        // RLS: modules_policy_teacher_all_own (Teacher chỉ xóa modules của own courses)
        [HttpDelete("{moduleId}")]
        public async Task<IActionResult> DeleteModule(int moduleId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xóa module {ModuleId}", teacherId, moduleId);

            // RLS đã filter: Teacher chỉ xóa được modules của own courses
            // Nếu module không thuộc own course → RLS sẽ filter → module == null → service trả về 404
            var result = await _moduleService.DeleteModule(moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

