using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.AdminManagement;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class AssetFrontendService : IAssetFrontendService
    {

        private readonly IMapper _mapper;
        private readonly IAssetFrontendMediaService _assetFrontendMediaService;

        private readonly IAssetFrontendRepository _assetFrontendRepository;
        private readonly ILogger<AssetFrontendService> _logger;

        public AssetFrontendService(
            IMapper mapper,
            IAssetFrontendMediaService assetFrontendMediaService,
            IAssetFrontendRepository assetFrontendRepository,
            ILogger<AssetFrontendService> logger
            )
        {
            _mapper = mapper;
            _assetFrontendMediaService = assetFrontendMediaService;
            _assetFrontendRepository = assetFrontendRepository;
            _logger = logger;
        }
        public async Task<ServiceResponse<List<AssetFrontendDto>>> GetAllAssetFrontends()
        {
            var response = new ServiceResponse<List<AssetFrontendDto>>();
            try
            {
                var assetFrontends = await _assetFrontendRepository.GetAllAssetFrontend();
                response.Data = _mapper.Map<List<AssetFrontendDto>>(assetFrontends);

                // Thêm ImageUrl cho từng asset
                foreach (var asset in response.Data)
                {
                    if (!string.IsNullOrWhiteSpace(asset.KeyImage))
                    {
                        asset.ImageUrl = _assetFrontendMediaService.BuildImageUrl(asset.KeyImage);
                    }
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy danh sách Asset Frontend thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi lấy danh sách Asset Frontend";
                _logger.LogError(ex, "Lỗi khi lấy danh sách Asset Frontend");
            }
            return response;
        }
        public async Task<ServiceResponse<AssetFrontendDto>> AddAssetFrontend(CreateAssetFrontendDto newAssetFrontend)
        {
            var response = new ServiceResponse<AssetFrontendDto>();
            try
            {
                // Kiểm tra xem đã có asset với AssetType này chưa (chỉ cho phép 1 asset mỗi loại)
                var existingAsset = await _assetFrontendRepository.GetAssetByType(newAssetFrontend.AssetType);
                if (existingAsset != null)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Đã tồn tại asset loại {newAssetFrontend.AssetType}. Chỉ cho phép 1 asset mỗi loại. Vui lòng cập nhật asset hiện có thay vì tạo mới.";
                    return response;
                }

                // Tạo asset entity
#pragma warning disable CS8601 // Possible null reference assignment
                var assetFrontend = new AssetFrontend
                {
                    NameImage = newAssetFrontend.NameImage,
                    AssetType = newAssetFrontend.AssetType
                };


                string? committedImageKey = null;

                // Convert temp file → real file nếu có ImageTempKey
                if (!string.IsNullOrWhiteSpace(newAssetFrontend.ImageTempKey))
                {
                    try
                    {
                        committedImageKey = await _assetFrontendMediaService.CommitImageAsync(newAssetFrontend.ImageTempKey);
                        assetFrontend.KeyImage = committedImageKey;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to commit asset frontend image");
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu ảnh asset. Vui lòng thử lại.";
                        return response;
                    }
                }

                try
                {
                    var addedAssetFrontend = await _assetFrontendRepository.AddAssetFrontend(assetFrontend);
                    response.Data = _mapper.Map<AssetFrontendDto>(addedAssetFrontend);

                    // Generate URL từ key
                    if (!string.IsNullOrWhiteSpace(addedAssetFrontend.KeyImage))
                    {
                        response.Data.ImageUrl = _assetFrontendMediaService.BuildImageUrl(addedAssetFrontend.KeyImage);
                    }

                    response.Success = true;
                    response.StatusCode = 201;
                    response.Message = "Thêm mới Asset Frontend thành công";
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while creating asset frontend");

                    // Rollback MinIO file
                    if (committedImageKey != null)
                    {
                        await _assetFrontendMediaService.DeleteImageAsync(committedImageKey);
                    }

                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = "Lỗi database khi tạo asset frontend";
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi thêm mới Asset Frontend";
                _logger.LogError(ex, "Lỗi khi thêm mới Asset Frontend");
            }
            return response;
        }
        public async Task<ServiceResponse<bool>> DeleteAssetFrontend(int id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var deletedAssetFrontend = await _assetFrontendRepository.DeleteAssetFrontend(id);
                if (deletedAssetFrontend == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Asset Frontend không tồn tại";
                    response.Data = false;
                    return response;
                }

                // Xóa file từ MinIO nếu có
                if (!string.IsNullOrWhiteSpace(deletedAssetFrontend.KeyImage))
                {
                    await _assetFrontendMediaService.DeleteImageAsync(deletedAssetFrontend.KeyImage);
                }

                response.Data = true;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Xóa Asset Frontend thành công";
            }
            catch (ArgumentException argEx)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = argEx.Message;
                response.Data = false;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi xóa Asset Frontend";
                response.Data = false;
                _logger.LogError(ex, "Lỗi khi xóa Asset Frontend");
            }
            return response;
        }
        public async Task<ServiceResponse<bool>> UpdateAssetFrontend(UpdateAssetFrontendDto updatedAssetFrontend)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                // Lấy asset hiện tại để rollback nếu cần
                var existingAsset = await _assetFrontendRepository.GetAssetFrontendById(updatedAssetFrontend.Id);
                if (existingAsset == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Asset Frontend không tồn tại";
                    response.Data = false;
                    return response;
                }

                // Nếu đang thay đổi AssetType, kiểm tra xem AssetType mới đã có asset chưa
                if (updatedAssetFrontend.AssetType.HasValue && updatedAssetFrontend.AssetType.Value != existingAsset.AssetType)
                {
                    var assetWithNewType = await _assetFrontendRepository.GetAssetByType(updatedAssetFrontend.AssetType.Value);
                    if (assetWithNewType != null && assetWithNewType.Id != existingAsset.Id)
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = $"Đã tồn tại asset loại {updatedAssetFrontend.AssetType.Value}. Chỉ cho phép 1 asset mỗi loại.";
                        response.Data = false;
                        return response;
                    }
                }

                string? oldImageKey = existingAsset.KeyImage;
                string? committedImageKey = null;

                // Convert temp file → real file nếu có ImageTempKey mới
                if (!string.IsNullOrWhiteSpace(updatedAssetFrontend.ImageTempKey))
                {
                    try
                    {
                        committedImageKey = await _assetFrontendMediaService.CommitImageAsync(updatedAssetFrontend.ImageTempKey);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to commit asset frontend image");
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu ảnh asset mới. Vui lòng thử lại.";
                        return response;
                    }
                }

                try
                {
                    // Cập nhật entity bằng AutoMapper với condition để chỉ map các field không null
                    _mapper.Map(updatedAssetFrontend, existingAsset);

                    // Xử lý KeyImage riêng biệt vì logic phức tạp hơn (commit file)
                    if (committedImageKey != null)
                        existingAsset.KeyImage = committedImageKey;

                    await _assetFrontendRepository.UpdateAssetFrontend(existingAsset);

                    // Xóa ảnh cũ nếu có ảnh mới
                    if (oldImageKey != null && committedImageKey != null)
                    {
                        await _assetFrontendMediaService.DeleteImageAsync(oldImageKey);
                    }

                    response.Data = true;
                    response.Success = true;
                    response.StatusCode = 200;
                    response.Message = "Cập nhật Asset Frontend thành công";
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while updating asset frontend");

                    // Rollback MinIO file mới
                    if (committedImageKey != null)
                    {
                        await _assetFrontendMediaService.DeleteImageAsync(committedImageKey);
                    }

                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = "Lỗi database khi cập nhật asset frontend";
                    response.Data = false;
                    return response;
                }
            }
            catch (ArgumentException argEx)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = argEx.Message;
                response.Data = false;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi cập nhật Asset Frontend";
                response.Data = false;
                _logger.LogError(ex, "Lỗi khi cập nhật Asset Frontend");
            }
            return response;
        }

        // Public methods - chỉ lấy assets đang active
        public async Task<ServiceResponse<List<AssetFrontendDto>>> GetAllActiveAssetFrontends()
        {
            var response = new ServiceResponse<List<AssetFrontendDto>>();
            try
            {
                var assetFrontends = await _assetFrontendRepository.GetAllActiveAssetFrontend();
                response.Data = _mapper.Map<List<AssetFrontendDto>>(assetFrontends);

                // Thêm ImageUrl cho từng asset
                foreach (var asset in response.Data)
                {
                    if (!string.IsNullOrWhiteSpace(asset.KeyImage))
                    {
                        asset.ImageUrl = _assetFrontendMediaService.BuildImageUrl(asset.KeyImage);
                    }
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy danh sách Asset Frontend active thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi lấy danh sách Asset Frontend active";
                _logger.LogError(ex, "Lỗi khi lấy danh sách Asset Frontend active");
            }
            return response;
        }

    }
}