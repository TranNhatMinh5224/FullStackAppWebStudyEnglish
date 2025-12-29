using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.Services.Lecture;
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
        private readonly ITeacherLectureCommandService _commandService;
        private readonly ITeacherLectureQueryService _queryService;
        private readonly ILogger<TeacherLectureController> _logger;

        public TeacherLectureController(
            ITeacherLectureCommandService commandService,
            ITeacherLectureQueryService queryService,
            ILogger<TeacherLectureController> logger)
        {
            _commandService = commandService;
            _queryService = queryService;
            _logger = logger;
        }

        // POST: api/teacher/lectures - Teacher tạo lecture
       
        [HttpPost]
        public async Task<IActionResult> CreateLecture([FromBody] CreateLectureDto createLectureDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang tạo lecture cho module {ModuleId}", teacherId, createLectureDto.ModuleId);

            var result = await _commandService.TeacherCreateLecture(createLectureDto, teacherId);
            return result.Success
                ? CreatedAtAction(nameof(GetLecture), new { lectureId = result.Data?.LectureId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // POST: api/teacher/lectures/bulk - Teacher tạo nhiều lectures cùng lúc
        
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreateLectures([FromBody] BulkCreateLecturesDto bulkCreateDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang bulk create lectures cho module {ModuleId}", teacherId, bulkCreateDto.ModuleId);

            var result = await _commandService.TeacherBulkCreateLectures(bulkCreateDto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/lectures/{lectureId} - Teacher xem chi tiết lecture
       
        [HttpGet("{lectureId}")]
        public async Task<IActionResult> GetLecture(int lectureId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem lecture {LectureId}", teacherId, lectureId);

            
            var result = await _queryService.GetLectureByIdAsync(lectureId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/lectures/module/{moduleId} - Teacher xem danh sách lectures theo module
        
        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetLecturesByModule(int moduleId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem danh sách lectures của module {ModuleId}", teacherId, moduleId);

            
            var result = await _queryService.GetLecturesByModuleIdAsync(moduleId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        [HttpGet("module/{moduleId}/tree")]
        public async Task<IActionResult> GetLectureTree(int moduleId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem cây lecture của module {ModuleId}", teacherId, moduleId);

            var result = await _queryService.GetLectureTreeByModuleIdAsync(moduleId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        
        [HttpPut("{lectureId}")]
        public async Task<IActionResult> UpdateLecture(int lectureId, [FromBody] UpdateLectureDto updateLectureDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang cập nhật lecture {LectureId}", teacherId, lectureId);

            var result = await _commandService.UpdateLecture(lectureId, updateLectureDto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/teacher/lectures/{lectureId} - Teacher xóa lecture

        [HttpDelete("{lectureId}")]
        public async Task<IActionResult> DeleteLecture(int lectureId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xóa lecture {LectureId}", teacherId, lectureId);

            var result = await _commandService.DeleteLecture(lectureId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/teacher/lectures/reorder - Teacher sắp xếp lại thứ tự lectures
       
        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderLectures([FromBody] List<ReorderLectureDto> reorderDtos)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang reorder lectures", teacherId);

            var result = await _commandService.ReorderLectures(reorderDtos, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

