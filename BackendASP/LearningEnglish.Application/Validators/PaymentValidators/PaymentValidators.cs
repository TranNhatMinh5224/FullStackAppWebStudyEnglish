using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.Payment
{
    public class RequestPaymentValidator : AbstractValidator<requestPayment>
    {
        public RequestPaymentValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("ProductId must be greater than 0");

            RuleFor(x => x.typeproduct)
                .IsInEnum().WithMessage("Invalid TypeProduct");
        }
    }

    public class CompletePaymentValidator : AbstractValidator<CompletePayment>
    {
        public CompletePaymentValidator()
        {
            RuleFor(x => x.PaymentId)
                .GreaterThan(0).WithMessage("PaymentId must be greater than 0");

            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("ProductId must be greater than 0");

            RuleFor(x => x.ProductType)
                .IsInEnum().WithMessage("Invalid TypeProduct");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("PaymentMethod is required")
                .MaximumLength(50).WithMessage("PaymentMethod must not exceed 50 characters");
        }
    }
}
