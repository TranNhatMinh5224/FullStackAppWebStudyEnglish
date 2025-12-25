
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Versioning;
namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/admin-teacher/asset-frontend")]
   
    public class AdminAssetFrontendController : ControllerBase
    {
        private readonly IAssetFrontendService _assetFrontendService; //  khai báo biến  _assetFrontendService kiểu IAssetFrontendService
        private readonly ILogger<AdminAssetFrontendController> _logger;
        public AdminAssetFrontendController(IAssetFrontendService assetFrontendService, ILogger<AdminAssetFrontendController> logger) //  khi gọi controller này constructor thấy  cần  assetFrontendService .DI container inject implementation tương ứng 
        {
            _assetFrontendService = assetFrontendService; // gán giá trị biến  _assetFrontendService
            _logger = logger;
        }
        [HttpGet("Get-all-asset-frontends")]
        
        public async Task<IActionResult> GetAllAssetFrontends()
        {
            var result = await _assetFrontendService.GetAllAssetFrontends();

            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
            // nếu thành công trả về 200 và data, ngược lại trả về mã lỗi và message


        }
        [HttpGet("Get-asset-frontend-by-id/{id}")]

        public async Task<IActionResult> GetAssetFrontendById(int id)
        {
            var result = await _assetFrontendService.GetAssetFrontendById(id);

            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
            // nếu thành công trả về 200 và data, ngược lại trả về mã lỗi và message

        }
        [HttpGet("Get-assets-by-type/{assetType}")]
        public async Task<IActionResult> GetAssetsByType(int assetType)
        {
            var result = await _assetFrontendService.GetAssetsByTypeAsync(assetType);

            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });

        }
        [HttpPost("Add-asset-frontend")]
         [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> AddAssetFrontend([FromBody] CreateAssetFrontendDto newAssetFrontend)
        {
            var result = await _assetFrontendService.AddAssetFrontend(newAssetFrontend);

            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });

        }
        [HttpPut("Update-asset-frontend")]
         [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateAssetFrontend([FromBody] UpdateAssetFrontendDto updatedAssetFrontend)
        {
            var result = await _assetFrontendService.UpdateAssetFrontend(updatedAssetFrontend);

            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });

        }
        [HttpDelete("Delete-asset-frontend/{id}")]
         [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> DeleteAssetFrontend(int id)
        {
            var result = await _assetFrontendService.DeleteAssetFrontend(id);

            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });

        }
    }
}