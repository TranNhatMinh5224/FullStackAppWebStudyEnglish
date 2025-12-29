using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class PaymentValidator : IPaymentValidator
    {
        private readonly IUserRepository _userRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IEnumerable<IPaymentStrategy> _paymentStrategies;
        private readonly ILogger<PaymentValidator> _logger;

        public PaymentValidator(
            IUserRepository userRepository,
            IPaymentRepository paymentRepository,
            IEnumerable<IPaymentStrategy> paymentStrategies,
            ILogger<PaymentValidator> logger)
        {
            _userRepository = userRepository;
            _paymentRepository = paymentRepository;
            _paymentStrategies = paymentStrategies;
            _logger = logger;
        }

        public async Task<ServiceResponse<decimal>> ValidateProductAsync(int productId, ProductType productType)
        {
            var response = new ServiceResponse<decimal>();

            try
            {
                // Find the appropriate strategy for this product type
                var processor = _paymentStrategies.FirstOrDefault(s => s.ProductType == productType);
                if (processor == null)
                {
                    _logger.LogWarning("No payment strategy found for product type {ProductType}", productType);
                    response.Success = false;
                    response.Message = "Loại sản phẩm không được hỗ trợ";
                    return response;
                }

                // Delegate validation to the strategy (DRY principle)
                return await processor.ValidateProductAsync(productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi validate sản phẩm {ProductId}, Loại {ProductType}", productId, productType);
                response.Success = false;
                response.Message = "Đã xảy ra lỗi khi validate sản phẩm";
                return response;
            }
        }

        public async Task<ServiceResponse<bool>> ValidateUserPaymentAsync(int userId, int productId, ProductType productType)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Kiểm tra user tồn tại
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Không tìm thấy User {UserId}", userId);
                    response.Success = false;
                    response.Message = "Không tìm thấy người dùng";
                    return response;
                }

                // Kiểm tra user đã mua sản phẩm này chưa
                var existingPayment = await _paymentRepository.GetSuccessfulPaymentByUserAndProductAsync(userId, productId, productType);
                if (existingPayment != null)
                {
                    _logger.LogWarning("User {UserId} đã mua {ProductType} {ProductId}", userId, productType, productId);
                    response.Success = false;
                    response.Message = "Bạn đã mua sản phẩm này rồi";
                    return response;
                }

                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi validate user payment cho User {UserId}", userId);
                response.Success = false;
                response.Message = "Đã xảy ra lỗi khi kiểm tra thông tin thanh toán";
            }

            return response;
        }
    }
}
