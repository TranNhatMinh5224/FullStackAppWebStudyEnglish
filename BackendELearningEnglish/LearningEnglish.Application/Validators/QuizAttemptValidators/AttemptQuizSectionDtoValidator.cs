using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.QuizAttemptValidators;

public class AttemptQuizSectionDtoValidator : AbstractValidator<AttemptQuizSectionDto>
{
    public AttemptQuizSectionDtoValidator()
    {
        RuleFor(x => x.SectionId)
            .GreaterThan(0)
            .WithMessage("SectionId phải lớn hơn 0");

        RuleFor(x => x.Title)
            .NotNull()
            .NotEmpty()
            .WithMessage("Title không được null hoặc rỗng");

        RuleFor(x => x.Items)
            .NotNull()
            .WithMessage("Items không được null");
    }
}
