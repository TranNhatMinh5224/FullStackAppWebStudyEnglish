using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.VocabularyReviewValidators;

public class SubmitReviewRequestDtoValidator : AbstractValidator<SubmitReviewRequestDto>
{
    public SubmitReviewRequestDtoValidator()
    {
        RuleFor(x => x.Quality)
            .InclusiveBetween(0, 5)
            .WithMessage("Quality phải từ 0 đến 5");
    }
}
