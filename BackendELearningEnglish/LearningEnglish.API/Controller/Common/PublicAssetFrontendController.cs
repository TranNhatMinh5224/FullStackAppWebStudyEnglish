using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.AdminManagement;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Common
{
    /// <summary>
    /// Public API Controller cho Asset Frontend
    /// Không cần authentication - dành cho frontend lấy assets để hiển thị
    /// </summary>
    [ApiController]
    [Route("api/public/asset-frontend")]
    public class PublicAssetFrontendController : ControllerBase
    {
        private readonly IAssetFrontendService _assetFrontendService;

        public PublicAssetFrontendController(IAssetFrontendService assetFrontendService)
        {
            _assetFrontendService = assetFrontendService;
        }

        /// <summary>
        /// Lấy tất cả assets đang active (public - không cần auth)
        /// GET: api/public/asset-frontend
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ServiceResponse<List<AssetFrontendDto>>), 200)]
        public async Task<IActionResult> GetAllActiveAssets()
        {
            var result = await _assetFrontendService.GetAllActiveAssetFrontends();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

    }
}
