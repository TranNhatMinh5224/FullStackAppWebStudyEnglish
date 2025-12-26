using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class AssetFrontendService : IAssetFrontendService
    {

        private readonly IMapper _mapper;
        private readonly IMinioFileStorage _minioFileStorage;

        private readonly IAssetFrontendRepository _assetFrontendRepository;
        private readonly ILogger<AssetFrontendService> _logger;

        // Đặt bucket + folder cho ảnh asset frontend
        private const string AssetImageBucket = "assetsfrontend";
        private const string AssetImageFolder = "real";

        public AssetFrontendService(
            IMapper mapper,
            IMinioFileStorage minioFileStorage,
            IAssetFrontendRepository assetFrontendRepository,
            ILogger<AssetFrontendService> logger
            )
        {
            _mapper = mapper;
            _minioFileStorage = minioFileStorage;
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
                        asset.ImageUrl = BuildPublicUrl.BuildURL("assets", asset.KeyImage);
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
        public async Task<ServiceResponse<AssetFrontendDto>> GetAssetFrontendById(int id)
        {
            var response = new ServiceResponse<AssetFrontendDto>();
            try
            {
                var assetFrontend = await _assetFrontendRepository.GetAssetFrontendById(id);
                if (assetFrontend == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Asset Frontend không tồn tại";
                    return response;
                }
                response.Data = _mapper.Map<AssetFrontendDto>(assetFrontend);

                // Thêm ImageUrl
                if (!string.IsNullOrWhiteSpace(response.Data.KeyImage))
                {
                    response.Data.ImageUrl = BuildPublicUrl.BuildURL("assets", response.Data.KeyImage);
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy thông tin Asset Frontend thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi lấy thông tin Asset Frontend";
                _logger.LogError(ex, "Lỗi khi lấy thông tin Asset Frontend");
            }
            return response;


        }
        public async Task<ServiceResponse<List<AssetFrontendDto>>> GetAssetsByTypeAsync(int assetType)
        {
            var response = new ServiceResponse<List<AssetFrontendDto>>();
            try
            {
                var assetFrontends = await _assetFrontendRepository.GetAssetByType(assetType);
                response.Data = _mapper.Map<List<AssetFrontendDto>>(assetFrontends);

                // Thêm ImageUrl cho từng asset
                foreach (var asset in response.Data)
                {
                    if (!string.IsNullOrWhiteSpace(asset.KeyImage))
                    {
                        asset.ImageUrl = BuildPublicUrl.BuildURL("assets", asset.KeyImage);
                    }
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy danh sách Asset Frontend theo loại thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi lấy danh sách Asset Frontend theo loại";
                _logger.LogError(ex, "Lỗi khi lấy danh sách Asset Frontend theo loại");
            }
            return response;
        }
        public async Task<ServiceResponse<AssetFrontendDto>> AddAssetFrontend(CreateAssetFrontendDto newAssetFrontend)
        {
            var response = new ServiceResponse<AssetFrontendDto>();
            try
            {
                // Tạo asset entity
                var assetFrontend = new AssetFrontend
                {
                    NameImage = newAssetFrontend.NameImage,
                    DescriptionImage = newAssetFrontend.DescriptionImage,
                    AssetType = newAssetFrontend.AssetType.ToString()!,
                    Order = newAssetFrontend.Order,
                    IsActive = newAssetFrontend.IsActive
                };

                string? committedImageKey = null;

                // Convert temp file → real file nếu có ImageTempKey
                if (!string.IsNullOrWhiteSpace(newAssetFrontend.ImageTempKey))
                {
                    var commitResult = await _minioFileStorage.CommitFileAsync(
                        newAssetFrontend.ImageTempKey,
                        AssetImageBucket,
                        AssetImageFolder
                    );

                    if (!commitResult.Success || string.IsNullOrWhiteSpace(commitResult.Data))
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu ảnh asset. Vui lòng thử lại.";
                        return response;
                    }

                    committedImageKey = commitResult.Data;
                    assetFrontend.KeyImage = committedImageKey;
                }

                try
                {
                    var addedAssetFrontend = await _assetFrontendRepository.AddAssetFrontend(assetFrontend);
                    response.Data = _mapper.Map<AssetFrontendDto>(addedAssetFrontend);

                    // Generate URL từ key
                    if (!string.IsNullOrWhiteSpace(addedAssetFrontend.KeyImage))
                    {
                        response.Data.ImageUrl = BuildPublicUrl.BuildURL("assets", addedAssetFrontend.KeyImage);
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
                        await _minioFileStorage.DeleteFileAsync(committedImageKey, "assets");
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
                    await _minioFileStorage.DeleteFileAsync(deletedAssetFrontend.KeyImage, "assets");
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

                string? oldImageKey = existingAsset.KeyImage;
                string? committedImageKey = null;

                // Convert temp file → real file nếu có ImageTempKey mới
                if (!string.IsNullOrWhiteSpace(updatedAssetFrontend.ImageTempKey))
                {
                    var commitResult = await _minioFileStorage.CommitFileAsync(
                        updatedAssetFrontend.ImageTempKey,
                        AssetImageBucket,
                        AssetImageFolder
                    );

                    if (!commitResult.Success || string.IsNullOrWhiteSpace(commitResult.Data))
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu ảnh asset mới. Vui lòng thử lại.";
                        return response;
                    }

                    committedImageKey = commitResult.Data;
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
                        await _minioFileStorage.DeleteFileAsync(oldImageKey, "assets");
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
                        await _minioFileStorage.DeleteFileAsync(committedImageKey, "assets");
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
    }
}