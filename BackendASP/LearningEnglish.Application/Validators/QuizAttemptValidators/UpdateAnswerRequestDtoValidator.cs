using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.QuizAttemptValidators;

public class UpdateAnswerRequestDtoValidator : AbstractValidator<UpdateAnswerRequestDto>
{
    public UpdateAnswerRequestDtoValidator()
    {
        RuleFor(x => x.QuestionId)
            .GreaterThan(0)
            .WithMessage("QuestionId phải lớn hơn 0");

        RuleFor(x => x.UserAnswer)
            .NotNull()
            .WithMessage("UserAnswer không được null")
            .When(x => x.UserAnswer != null); // Có thể null nếu xóa answer, nhưng nếu có thì validate

        // Có thể thêm validate type của UserAnswer dựa trên question type, nhưng cần context
    }
}
