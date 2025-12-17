using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/public/dictionary")]
    [Authorize]
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

        // GET: api/public/dictionary/lookup/{word} - xem định nghĩa từ
        [HttpGet("lookup/{word}")]
        [ProducesResponseType(typeof(DictionaryLookupResultDto), 200)]
        public async Task<IActionResult> LookupWord(string word, [FromQuery] string? targetLanguage = "vi")
        {
            var result = await _dictionaryService.LookupWordAsync(word, targetLanguage);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/public/dictionary/generate-flashcard - tạo flashcard dựa trên từ được cung cấp
        [HttpPost("generate-flashcard")]
        [ProducesResponseType(typeof(GenerateFlashCardPreviewResponseDto), 200)]
        public async Task<IActionResult> GenerateFlashCard([FromBody] GenerateFlashCardRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Word))
            {
                return BadRequest(new { message = "Word is required" });
            }

            var result = await _dictionaryService.GenerateFlashCardFromWordAsync(request.Word);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

