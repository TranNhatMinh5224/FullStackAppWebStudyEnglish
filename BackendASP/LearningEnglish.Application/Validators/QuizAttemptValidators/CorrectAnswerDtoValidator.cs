using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.QuizAttemptValidators;

public class CorrectAnswerDtoValidator : AbstractValidator<CorrectAnswerDto>
{
    public CorrectAnswerDtoValidator()
    {
        RuleFor(x => x.QuestionId)
            .GreaterThan(0)
            .WithMessage("QuestionId phải lớn hơn 0");

        RuleFor(x => x.QuestionText)
            .NotNull()
            .NotEmpty()
            .WithMessage("QuestionText không được null hoặc rỗng");

        RuleFor(x => x.CorrectOptions)
            .NotNull()
            .WithMessage("CorrectOptions không được null")
            .NotEmpty()
            .WithMessage("CorrectOptions không được rỗng");
    }
}
