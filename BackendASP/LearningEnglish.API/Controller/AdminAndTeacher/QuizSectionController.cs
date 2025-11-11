using FluentValidation;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controllers.AdminAndTeacher
{
    [ApiController]
    [Route("api/admin/quiz-sections")]
    [Authorize(Roles = "Admin,Teacher")]
    public class QuizSectionController : ControllerBase
    {
        private readonly IQuizSectionService _quizSectionService;
        private readonly IValidator<CreateQuizSectionDto> _createValidator;
        private readonly IValidator<UpdateQuizSectionDto> _updateValidator;

        public QuizSectionController(
            IQuizSectionService quizSectionService,
            IValidator<CreateQuizSectionDto> createValidator,
            IValidator<UpdateQuizSectionDto> updateValidator)
        {
            _quizSectionService = quizSectionService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        /// <summary>
        /// Tạo phần quiz mới
        /// </summary>
        /// <param name="createDto">Thông tin phần quiz mới</param>
        /// <returns>Phần quiz đã tạo</returns>
        [HttpPost]
        public async Task<IActionResult> CreateQuizSection([FromBody] CreateQuizSectionDto createDto)
        {
            try
            {
                var validationResult = await _createValidator.ValidateAsync(createDto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
                }

                var result = await _quizSectionService.CreateQuizSectionAsync(createDto);

                if (result.Success)
                    return CreatedAtAction(nameof(GetQuizSectionById), new { id = result.Data?.QuizSectionId }, result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuizSectionById(int id)
        {
            try
            {
                var result = await _quizSectionService.GetQuizSectionByIdAsync(id);

                if (result.Success)
                    return Ok(result);

                return NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách phần quiz theo Quiz ID
        /// </summary>
        /// <param name="quizId">ID của quiz</param>
        /// <returns>Danh sách phần quiz</returns>
        [HttpGet("by-quiz/{quizId}")]
        public async Task<IActionResult> GetQuizSectionsByQuizId(int quizId)
        {
            try
            {
                var result = await _quizSectionService.GetQuizSectionsByQuizIdAsync(quizId);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuizSection(int id, [FromBody] UpdateQuizSectionDto updateDto)
        {
            try
            {
                var validationResult = await _updateValidator.ValidateAsync(updateDto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
                }

                var result = await _quizSectionService.UpdateQuizSectionAsync(id, updateDto);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa phần quiz
        /// </summary>
        /// <param name="id">ID của phần quiz</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuizSection(int id)
        {
            try
            {
                var result = await _quizSectionService.DeleteQuizSectionAsync(id);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
