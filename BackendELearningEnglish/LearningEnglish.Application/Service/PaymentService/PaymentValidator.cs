using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.PaymentService
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

                // Kiểm tra duplicate payment
                if (productType == ProductType.Course)
                {
                    // Course: Không cho mua lại nếu đã enrolled
                    var existingPayment = await _paymentRepository.GetSuccessfulPaymentByUserAndProductAsync(userId, productId, productType);
                    if (existingPayment != null)
                    {
                        _logger.LogWarning("User {UserId} đã mua Course {ProductId}", userId, productId);
                        response.Success = false;
                        response.Message = "Bạn đã mua khóa học này rồi";
                        return response;
                    }
                }
                else if (productType == ProductType.TeacherPackage)
                {
                    // TeacherPackage: Chỉ cho phép mua KHI KHÔNG CÓ subscription nào active/pending
                    // Không phụ thuộc vào packageId cụ thể - chỉ check có subscription hay không
                    var existingPayment = await _paymentRepository.GetSuccessfulPaymentByUserAndProductAsync(userId, productId, productType);
                    if (existingPayment != null)
                    {
                        _logger.LogWarning("User {UserId} đã có TeacherPackage subscription đang hoạt động hoặc chờ kích hoạt", userId);
                        response.Success = false;
                        response.Message = "Bạn đã có gói giáo viên đang hoạt động. Vui lòng đợi gói hiện tại hết hạn trước khi mua gói mới";
                        return response;
                    }
                }

                response.Success = true;
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
