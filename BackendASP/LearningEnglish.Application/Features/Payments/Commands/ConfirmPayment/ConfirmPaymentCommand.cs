using MediatR;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Features.Payments.Commands.ConfirmPayment
{
    public class ConfirmPaymentCommand : IRequest<ServiceResponse<bool>>
    {
        public CompletePayment PaymentDto { get; set; }
        public int UserId { get; set; }

        public ConfirmPaymentCommand(CompletePayment paymentDto, int userId)
        {
            PaymentDto = paymentDto;
            UserId = userId;
        }
    }
}
