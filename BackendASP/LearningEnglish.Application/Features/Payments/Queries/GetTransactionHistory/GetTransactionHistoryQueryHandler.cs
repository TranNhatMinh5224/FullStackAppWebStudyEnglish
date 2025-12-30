using MediatR;
using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Strategies;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Features.Payments.Queries.GetTransactionHistory
{
    public class GetTransactionHistoryQueryHandler : IRequestHandler<GetTransactionHistoryQuery, ServiceResponse<PagedResult<TransactionHistoryDto>>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IEnumerable<IPaymentStrategy> _paymentStrategies;
        private readonly IMapper _mapper;
        private readonly ILogger<GetTransactionHistoryQueryHandler> _logger;

        public GetTransactionHistoryQueryHandler(
            IPaymentRepository paymentRepository,
            IEnumerable<IPaymentStrategy> paymentStrategies,
            IMapper mapper,
            ILogger<GetTransactionHistoryQueryHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _paymentStrategies = paymentStrategies;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResponse<PagedResult<TransactionHistoryDto>>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
        {
            var response = new ServiceResponse<PagedResult<TransactionHistoryDto>>();
            try
            {
                var totalCount = await _paymentRepository.GetTransactionCountAsync(request.UserId);
                var payments = await _paymentRepository.GetTransactionHistoryAsync(request.UserId, request.Request.PageNumber, request.Request.PageSize);

                var transactionDtos = new List<TransactionHistoryDto>();
                foreach (var payment in payments)
                {
                    var dto = _mapper.Map<TransactionHistoryDto>(payment);
                    var processor = _paymentStrategies.FirstOrDefault(s => s.ProductType == payment.ProductType);
                    dto.ProductName = processor != null 
                        ? await processor.GetProductNameAsync(payment.ProductId)
                        : "Sản phẩm";
                    transactionDtos.Add(dto);
                }

                response.Data = new PagedResult<TransactionHistoryDto>
                {
                    Items = transactionDtos,
                    TotalCount = totalCount,
                    PageNumber = request.Request.PageNumber,
                    PageSize = request.Request.PageSize
                };
                response.Success = true;
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching history");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Error fetching history";
            }
            return response;
        }
    }
}
