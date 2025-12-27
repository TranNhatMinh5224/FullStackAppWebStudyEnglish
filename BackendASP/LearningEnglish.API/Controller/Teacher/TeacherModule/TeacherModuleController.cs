using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.Services.Module;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Teacher quản lý modules

namespace LearningEnglish.API.Controller.Teacher
{
    [Route("api/teacher/modules")]
    [ApiController]
    [RequireTeacherRole]
    public class TeacherModuleController : ControllerBase
    {
        private readonly ITeacherModuleService _moduleService;
        private readonly ILogger<TeacherModuleController> _logger;

        public TeacherModuleController(ITeacherModuleService moduleService, ILogger<TeacherModuleController> logger)
        {
            _moduleService = moduleService;
            _logger = logger;
        }

        // POST: Teacher tạo module mới (own course only)
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

        // GET: Teacher xem chi tiết module (own course only)
        [HttpGet("{moduleId}")]
        public async Task<IActionResult> GetModule(int moduleId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem module {ModuleId}", teacherId, moduleId);

            var result = await _moduleService.GetModuleById(moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: Teacher lấy danh sách modules theo lesson (own course only)
        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetModulesByLesson(int lessonId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem danh sách modules của lesson {LessonId}", teacherId, lessonId);

            var result = await _moduleService.GetModulesByLessonId(lessonId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: Teacher cập nhật module (own course only)
        [HttpPut("{moduleId}")]
        public async Task<IActionResult> UpdateModule(int moduleId, [FromBody] UpdateModuleDto updateModuleDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang cập nhật module {ModuleId}", teacherId, moduleId);

            var result = await _moduleService.UpdateModule(moduleId, updateModuleDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: Teacher xóa module (own course only)
        [HttpDelete("{moduleId}")]
        public async Task<IActionResult> DeleteModule(int moduleId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xóa module {ModuleId}", teacherId, moduleId);

            var result = await _moduleService.DeleteModule(moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

