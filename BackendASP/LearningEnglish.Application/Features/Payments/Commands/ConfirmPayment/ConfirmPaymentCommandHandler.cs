using MediatR;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Features.Payments.Events;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Features.Payments.Commands.ConfirmPayment
{
    public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, ServiceResponse<bool>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ConfirmPaymentCommandHandler> _logger;
        private readonly IMediator _mediator;

        public ConfirmPaymentCommandHandler(
            IPaymentRepository paymentRepository,
            IUnitOfWork unitOfWork,
            ILogger<ConfirmPaymentCommandHandler> logger,
            IMediator mediator)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<ServiceResponse<bool>> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
        {
            var response = new ServiceResponse<bool>();
            var paymentDto = request.PaymentDto;
            var userId = request.UserId;

            try
            {
                var existingPayment = await _paymentRepository.GetPaymentByIdAsync(paymentDto.PaymentId);
                
                if (existingPayment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy thanh toán";
                    return response;
                }

                if (existingPayment.UserId != userId)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền xác nhận thanh toán này";
                    return response;
                }

                if (existingPayment.Status != PaymentStatus.Pending)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Thanh toán đã được xử lý";
                    return response;
                }

                if (existingPayment.Amount != paymentDto.Amount)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Số tiền thanh toán không khớp";
                    return response;
                }

                await _unitOfWork.BeginTransactionAsync();

                existingPayment.Status = PaymentStatus.Completed;
                existingPayment.PaidAt = DateTime.UtcNow;
                existingPayment.UpdatedAt = DateTime.UtcNow;

                await _paymentRepository.UpdatePaymentStatusAsync(existingPayment);
                await _paymentRepository.SaveChangesAsync();

                // DECOUPLING: Thay vì gọi strategy trực tiếp, bắn Event
                await _mediator.Publish(new PaymentCompletedEvent(existingPayment), cancellationToken);

                await _unitOfWork.CommitAsync();

                response.Success = true;
                response.StatusCode = 200;
                response.Data = true;
                response.Message = "Xác nhận thanh toán thành công";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Lỗi ConfirmPaymentCommand");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi xác nhận thanh toán";
            }

            return response;
        }
    }
}
