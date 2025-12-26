using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.API.Authorization;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/admin/asset-frontend")]
    [Authorize(Roles = "Admin")]
    public class AssetFrontendController : ControllerBase
    {
        private readonly IAssetFrontendService _assetFrontendService;

        public AssetFrontendController(IAssetFrontendService assetFrontendService)
        {
            _assetFrontendService = assetFrontendService;
        }

        // GET: api/admin/asset-frontend
        [HttpGet]
        [RequirePermission("Admin.Asset.Manage")]
        public async Task<IActionResult> GetAllAssetFrontends()
        {
            var result = await _assetFrontendService.GetAllAssetFrontends();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/asset-frontend/{id}
        [HttpGet("{id}")]
        [RequirePermission("Admin.Asset.Manage")]
        public async Task<IActionResult> GetAssetFrontendById(int id)
        {
            var result = await _assetFrontendService.GetAssetFrontendById(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/asset-frontend/type/{assetType}
        [HttpGet("type/{assetType}")]
        [RequirePermission("Admin.Asset.Manage")]
        public async Task<IActionResult> GetAssetsByType(int assetType)
        {
            var result = await _assetFrontendService.GetAssetsByTypeAsync(assetType);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/admin/asset-frontend
        [HttpPost]
        [RequirePermission("Admin.Asset.Manage")]
        public async Task<IActionResult> CreateAssetFrontend([FromBody] CreateAssetFrontendDto createDto)
        {
            var result = await _assetFrontendService.AddAssetFrontend(createDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/admin/asset-frontend/{id}
        [HttpPut("{id}")]
        [RequirePermission("Admin.Asset.Manage")]
        public async Task<IActionResult> UpdateAssetFrontend(int id, [FromBody] UpdateAssetFrontendDto updateDto)
        {
            updateDto.Id = id; // Ensure ID is set from route
            var result = await _assetFrontendService.UpdateAssetFrontend(updateDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/admin/asset-frontend/{id}
        [HttpDelete("{id}")]
        [RequirePermission("Admin.Asset.Manage")]
        public async Task<IActionResult> DeleteAssetFrontend(int id)
        {
            var result = await _assetFrontendService.DeleteAssetFrontend(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}