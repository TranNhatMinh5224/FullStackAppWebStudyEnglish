using MediatR;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Features.Payments.Commands.ConfirmPayOSPayment
{
    public class ConfirmPayOSPaymentCommand : IRequest<ServiceResponse<bool>>
    {
        public int PaymentId { get; set; }
        public int UserId { get; set; }

        public ConfirmPayOSPaymentCommand(int paymentId, int userId)
        {
            PaymentId = paymentId;
            UserId = userId;
        }
    }
}
