using MediatR;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Features.Payments.Events
{
    public class PaymentCompletedEvent : INotification
    {
        public Payment Payment { get; }

        public PaymentCompletedEvent(Payment payment)
        {
            Payment = payment;
        }
    }
}
