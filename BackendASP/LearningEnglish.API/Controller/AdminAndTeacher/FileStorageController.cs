using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller
{
    [Route("api/files")]
    [ApiController]
    [Authorize] // Yêu cầu đăng nhập
    public class FileStorageController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<FileStorageController> _logger;

        public FileStorageController(
            IFileStorageService fileStorageService,
            ILogger<FileStorageController> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // Upload file tạm lên MinIO
        [HttpPost("upload-temp")]
        public async Task<IActionResult> UploadTempFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File is required" });

            var response = await _fileStorageService.UploadTempFileAsync(file);
            
            if (response.Success)
                return Ok(response);
            
            return StatusCode(response.StatusCode, response);
        }

        // Chuyển file tạm thành file thật
        [HttpPost("convert-to-real")]
        public async Task<IActionResult> ConvertToRealFile([FromBody] ConvertTempToRealFileRequestDto request)
        {
            if (string.IsNullOrEmpty(request.TempKey))
                return BadRequest(new { message = "TempKey is required" });

            var response = await _fileStorageService.ConvertTempToRealFileAsync(
                request.TempKey, 
                request.RealFolderPath
            );
            
            if (response.Success)
                return Ok(response);
            
            return StatusCode(response.StatusCode, response);
        }

        // Xóa file tạm
        [HttpDelete("temp/{tempKey}")]
        public async Task<IActionResult> DeleteTempFile(string tempKey)
        {
            var response = await _fileStorageService.DeleteTempFileAsync(tempKey);
            
            if (response.Success)
                return Ok(response);
            
            return StatusCode(response.StatusCode, response);
        }

        // Xóa file thật
        [HttpDelete("real/{fileKey}")]
        public async Task<IActionResult> DeleteRealFile(string fileKey)
        {
            var response = await _fileStorageService.DeleteRealFileAsync(fileKey);
            
            if (response.Success)
                return Ok(response);
            
            return StatusCode(response.StatusCode, response);
        }

        // Lấy URL của file (presigned URL)
        [HttpGet("url/{fileKey}")]
        public async Task<IActionResult> GetFileUrl(string fileKey)
        {
            var response = await _fileStorageService.GetFileUrl(fileKey);
            
            if (response.Success)
                return Ok(response);
            
            return StatusCode(response.StatusCode, response);
        }

        // Kiểm tra file có tồn tại không
        [HttpGet("exists/{fileKey}")]
        public async Task<IActionResult> FileExists(string fileKey)
        {
            var response = await _fileStorageService.FileExistsAsync(fileKey);
            
            return Ok(response);
        }
    }
}

