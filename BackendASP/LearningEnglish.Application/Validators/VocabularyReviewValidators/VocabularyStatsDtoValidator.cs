using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.VocabularyReviewValidators;

public class VocabularyStatsDtoValidator : AbstractValidator<VocabularyStatsDto>
{
    public VocabularyStatsDtoValidator()
    {
        RuleFor(x => x.TotalCards)
            .GreaterThanOrEqualTo(0)
            .WithMessage("TotalCards không được âm");

        RuleFor(x => x.DueToday)
            .GreaterThanOrEqualTo(0)
            .WithMessage("DueToday không được âm");

        RuleFor(x => x.MasteredCards)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MasteredCards không được âm");

        RuleFor(x => x.LearningCards)
            .GreaterThanOrEqualTo(0)
            .WithMessage("LearningCards không được âm");

        RuleFor(x => x.NewCards)
            .GreaterThanOrEqualTo(0)
            .WithMessage("NewCards không được âm");

        RuleFor(x => x.TotalReviews)
            .GreaterThanOrEqualTo(0)
            .WithMessage("TotalReviews không được âm");

        RuleFor(x => x.AverageQuality)
            .InclusiveBetween(0, 5)
            .WithMessage("AverageQuality phải từ 0 đến 5");

        RuleFor(x => x.StreakDays)
            .GreaterThanOrEqualTo(0)
            .WithMessage("StreakDays không được âm");
    }
}
