using MediatR;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Features.Payments.Commands.ProcessPayOSReturn
{
    public class ProcessPayOSReturnCommand : IRequest<ServiceResponse<PayOSReturnResult>>
    {
        public string Code { get; set; }
        public string Desc { get; set; }
        public string Data { get; set; }
        public string? OrderCode { get; set; }
        public string? Status { get; set; }

        public ProcessPayOSReturnCommand(string code, string desc, string data, string? orderCode, string? status)
        {
            Code = code;
            Desc = desc;
            Data = data;
            OrderCode = orderCode;
            Status = status;
        }
    }
}
