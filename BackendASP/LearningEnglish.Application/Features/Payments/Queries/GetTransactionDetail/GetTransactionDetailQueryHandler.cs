using MediatR;
using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Strategies;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Features.Payments.Queries.GetTransactionDetail
{
    public class GetTransactionDetailQueryHandler : IRequestHandler<GetTransactionDetailQuery, ServiceResponse<TransactionDetailDto>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IEnumerable<IPaymentStrategy> _paymentStrategies;
        private readonly IMapper _mapper;
        private readonly ILogger<GetTransactionDetailQueryHandler> _logger;

        public GetTransactionDetailQueryHandler(
            IPaymentRepository paymentRepository,
            IEnumerable<IPaymentStrategy> paymentStrategies,
            IMapper mapper,
            ILogger<GetTransactionDetailQueryHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _paymentStrategies = paymentStrategies;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResponse<TransactionDetailDto>> Handle(GetTransactionDetailQuery request, CancellationToken cancellationToken)
        {
            var response = new ServiceResponse<TransactionDetailDto>();
            try
            {
                var payment = await _paymentRepository.GetTransactionDetailAsync(request.PaymentId, request.UserId);
                
                if (payment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Not found";
                    return response;
                }

                if (payment.UserId != request.UserId)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Forbidden";
                    return response;
                }

                var dto = _mapper.Map<TransactionDetailDto>(payment);
                var processor = _paymentStrategies.FirstOrDefault(s => s.ProductType == payment.ProductType);
                dto.ProductName = processor != null 
                    ? await processor.GetProductNameAsync(payment.ProductId)
                    : "Sản phẩm";

                response.Data = dto;
                response.Success = true;
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching detail");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Error fetching detail";
            }
            return response;
        }
    }
}
