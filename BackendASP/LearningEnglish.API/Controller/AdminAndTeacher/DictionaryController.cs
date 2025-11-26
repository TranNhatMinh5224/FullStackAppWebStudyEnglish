using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Teacher")]
    public class DictionaryController : ControllerBase
    {
        private readonly IDictionaryService _dictionaryService;
        private readonly ILogger<DictionaryController> _logger;

        public DictionaryController(
            IDictionaryService dictionaryService,
            ILogger<DictionaryController> logger)
        {
            _dictionaryService = dictionaryService;
            _logger = logger;
        }

        /// <summary>
        /// Lookup word in dictionary API - giúp người dùng tra cứu từ trong từ điển
        /// </summary>
        [HttpGet("lookup/{word}")]
        [ProducesResponseType(typeof(DictionaryLookupResultDto), 200)]
        public async Task<IActionResult> LookupWord(string word, [FromQuery] string? targetLanguage = "vi")
        {
            try
            {
                var result = await _dictionaryService.LookupWordAsync(word, targetLanguage);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up word: {Word}", word);
                return StatusCode(500, new { message = "An error occurred during word lookup" });
            }
        }

        /// <summary>
        /// Generate FlashCard preview from word - tạo preview flashcard từ 1 từ
        /// </summary>
        [HttpPost("generate-flashcard")]
        [ProducesResponseType(typeof(GenerateFlashCardPreviewResponseDto), 200)]
        public async Task<IActionResult> GenerateFlashCard([FromBody] GenerateFlashCardRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Word))
                {
                    return BadRequest(new { message = "Word is required" });
                }

                var result = await _dictionaryService.GenerateFlashCardFromWordAsync(request.Word);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating FlashCard from word: {Word}", request.Word);
                return StatusCode(500, new { message = "An error occurred while generating FlashCard" });
            }
        }
    }
}

