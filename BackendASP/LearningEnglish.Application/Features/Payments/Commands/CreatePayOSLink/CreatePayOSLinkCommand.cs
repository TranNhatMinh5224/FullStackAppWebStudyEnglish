using MediatR;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Features.Payments.Commands.CreatePayOSLink
{
    public class CreatePayOSLinkCommand : IRequest<ServiceResponse<PayOSLinkResponse>>
    {
        public int PaymentId { get; set; }
        public int UserId { get; set; }

        public CreatePayOSLinkCommand(int paymentId, int userId)
        {
            PaymentId = paymentId;
            UserId = userId;
        }
    }
}