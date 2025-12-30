using MediatR;
using LearningEnglish.Application.Features.Payments.Events;
using LearningEnglish.Application.Interface.Strategies;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Features.Payments.Handlers
{
    public class PaymentCompletedEventHandler : INotificationHandler<PaymentCompletedEvent>
    {
        private readonly IEnumerable<IPaymentStrategy> _paymentStrategies;
        private readonly ILogger<PaymentCompletedEventHandler> _logger;

        public PaymentCompletedEventHandler(
            IEnumerable<IPaymentStrategy> paymentStrategies,
            ILogger<PaymentCompletedEventHandler> logger)
        {
            _paymentStrategies = paymentStrategies;
            _logger = logger;
        }

        public async Task Handle(PaymentCompletedEvent notification, CancellationToken cancellationToken)
        {
            var payment = notification.Payment;
            _logger.LogInformation("Xử lý sự kiện PaymentCompleted cho Payment {PaymentId}, ProductType {Type}", 
                payment.PaymentId, payment.ProductType);

            try
            {
                var processor = _paymentStrategies.FirstOrDefault(s => s.ProductType == payment.ProductType);
                if (processor == null)
                {
                    _logger.LogError("Không tìm thấy strategy cho loại sản phẩm {Type}", payment.ProductType);
                    return;
                }

                // Thực hiện logic sau thanh toán (Enroll course, Activate package...)
                var result = await processor.ProcessPostPaymentAsync(
                    payment.UserId,
                    payment.ProductId,
                    payment.PaymentId);

                if (!result.Success)
                {
                    _logger.LogError("Lỗi Post-Payment Processing cho Payment {PaymentId}: {Message}", 
                        payment.PaymentId, result.Message);
                    // Ở đây có thể bắn thêm event PaymentProcessingFailedEvent nếu cần
                }
                else
                {
                    _logger.LogInformation("Post-Payment Processing thành công cho Payment {PaymentId}", payment.PaymentId);
                    // Có thể bắn thêm event EnrollmentSuccessEvent để gửi email...
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception trong PaymentCompletedEventHandler cho Payment {PaymentId}", payment.PaymentId);
                // Note: Exception trong Event Handler có thể không làm fail request chính nếu publish kiểu fire-and-forget, 
                // nhưng MediatR mặc định chạy tuần tự. Nếu muốn safe, cần try-catch kỹ.
            }
        }
    }
}
