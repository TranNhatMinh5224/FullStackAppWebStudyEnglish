using MediatR;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Features.Payments.Queries.GetTransactionDetail
{
    public class GetTransactionDetailQuery : IRequest<ServiceResponse<TransactionDetailDto>>
    {
        public int PaymentId { get; set; }
        public int UserId { get; set; }

        public GetTransactionDetailQuery(int paymentId, int userId)
        {
            PaymentId = paymentId;
            UserId = userId;
        }
    }
}
