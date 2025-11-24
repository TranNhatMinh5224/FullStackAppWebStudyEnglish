using LearningEnglish.Infrastructure.MinioFileStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/files")]
    [Authorize(Roles = "Admin, Teacher")]
    public class FilesController : ControllerBase
    {
        private readonly IMinioFileStorage _minioFileStorage;

        public FilesController(IMinioFileStorage minioFileStorage)
        {
            _minioFileStorage = minioFileStorage;
        }

        [HttpPost("temp-file")]
        public async Task<IActionResult> UploadTemplateFile(IFormFile file, [FromQuery] string bucketName, [FromQuery] string tempFolder = "temp")
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            if (string.IsNullOrEmpty(bucketName))
            {
                return BadRequest("Bucket name is required.");
            }
            var result = await _minioFileStorage.UpLoadFileTempAsync(file, bucketName, tempFolder);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);

        }
        [HttpDelete("temp-file")]
        [HttpDelete("temp")]
        public async Task<IActionResult> DeleteTemp(
            [FromQuery] string bucketName,
            [FromQuery] string tempKey)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
                return BadRequest("bucketName is required.");

            if (string.IsNullOrWhiteSpace(tempKey))
                return BadRequest("tempKey is required.");

            // TODO: validate bucketName (whitelist)

            var result = await _minioFileStorage.DeleteFileTempAsync(
                tempKey,
                bucketName
            );
            if (result.Success)
                return Ok(result);
            return BadRequest(result);

        }
    }
}
