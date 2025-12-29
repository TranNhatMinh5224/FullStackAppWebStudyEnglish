using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.QuizAttemptValidators
{
    public class UpdateAnswerRequestDtoValidator : AbstractValidator<UpdateAnswerRequestDto>
    {
        public UpdateAnswerRequestDtoValidator()
        {
            RuleFor(x => x.QuestionId)
                .GreaterThan(0)
                .WithMessage("QuestionId phải lớn hơn 0");

            RuleFor(x => x.UserAnswer)
                .NotNull()
                .WithMessage("UserAnswer không được null");
        }
    }
}

