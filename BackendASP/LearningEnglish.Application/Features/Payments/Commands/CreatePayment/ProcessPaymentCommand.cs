using MediatR;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Features.Payments.Commands.CreatePayment
{
    public class ProcessPaymentCommand : IRequest<ServiceResponse<CreateInforPayment>>
    {
        public int UserId { get; set; }
        public requestPayment Request { get; set; }

        public ProcessPaymentCommand(int userId, requestPayment request)
        {
            UserId = userId;
            Request = request;
        }
    }
}
