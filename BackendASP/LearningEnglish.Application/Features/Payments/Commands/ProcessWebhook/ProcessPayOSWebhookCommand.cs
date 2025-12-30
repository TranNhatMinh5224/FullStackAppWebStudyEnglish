using MediatR;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Features.Payments.Commands.ProcessWebhook
{
    public class ProcessPayOSWebhookCommand : IRequest<ServiceResponse<bool>>
    {
        public PayOSWebhookDto WebhookData { get; set; }
        public bool SkipSignatureCheck { get; set; }

        public ProcessPayOSWebhookCommand(PayOSWebhookDto webhookData, bool skipSignatureCheck = false)
        {
            WebhookData = webhookData;
            SkipSignatureCheck = skipSignatureCheck;
        }
    }
}
