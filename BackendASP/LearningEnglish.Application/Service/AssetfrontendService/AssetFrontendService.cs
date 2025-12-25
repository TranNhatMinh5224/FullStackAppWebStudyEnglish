using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
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
                var assetFrontend = _mapper.Map<AssetFrontend>(newAssetFrontend);
                var addedAssetFrontend = await _assetFrontendRepository.AddAssetFrontend(assetFrontend);
                response.Data = _mapper.Map<AssetFrontendDto>(addedAssetFrontend);
                response.Success = true;
                response.StatusCode = 201;
                response.Message = "Thêm mới Asset Frontend thành công";

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
                response.Data = true;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Xóa Asset Frontend thành công";
            }
            catch (ArgumentException argEx) // Xử lý lỗi khi không tìm thấy AssetFrontend lỗi này lấy ở repository
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
                var assetFrontend = _mapper.Map<AssetFrontend>(updatedAssetFrontend);
                await _assetFrontendRepository.UpdateAssetFrontend(assetFrontend);
                response.Data = true;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Cập nhật Asset Frontend thành công";
            }
            catch (ArgumentException argEx) // Xử lý lỗi khi không tìm thấy AssetFrontend lỗi này lấy ở repository
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