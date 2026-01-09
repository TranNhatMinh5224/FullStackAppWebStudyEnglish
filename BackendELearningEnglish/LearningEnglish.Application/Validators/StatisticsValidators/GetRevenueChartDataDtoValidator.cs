using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.StatisticsValidators
{
    public class GetRevenueChartDataRequestDtoValidator : AbstractValidator<GetRevenueChartDataRequestDto>
    {
        public GetRevenueChartDataRequestDtoValidator()
        {
            RuleFor(x => x.Days)
                .InclusiveBetween(1, 365)
                .WithMessage("Days must be between 1 and 365");
        }
    }
}

