using MediatR;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Features.Payments.Queries.GetTransactionHistory
{
    public class GetTransactionHistoryQuery : IRequest<ServiceResponse<PagedResult<TransactionHistoryDto>>>
    {
        public int UserId { get; set; }
        public PageRequest Request { get; set; }

        public GetTransactionHistoryQuery(int userId, PageRequest request)
        {
            UserId = userId;
            Request = request;
        }
    }
}
