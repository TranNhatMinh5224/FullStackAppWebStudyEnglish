using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.QuizAttemptValidators;

public class UpdateScoreRequestDtoValidator : AbstractValidator<UpdateScoreRequestDto>
{
    public UpdateScoreRequestDtoValidator()
    {
        RuleFor(x => x.AttemptId)
            .GreaterThan(0)
            .WithMessage("AttemptId phải lớn hơn 0");

        RuleFor(x => x.QuestionId)
            .GreaterThan(0)
            .WithMessage("QuestionId phải lớn hơn 0");

        RuleFor(x => x.UserAnswer)
            .NotNull()
            .WithMessage("UserAnswer không được null")
            .When(x => x.UserAnswer != null); // Tương tự
    }
}
