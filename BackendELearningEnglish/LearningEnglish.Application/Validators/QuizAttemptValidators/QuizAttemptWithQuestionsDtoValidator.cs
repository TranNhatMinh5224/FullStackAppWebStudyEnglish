using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.QuizAttemptValidators;

public class QuizAttemptWithQuestionsDtoValidator : AbstractValidator<QuizAttemptWithQuestionsDto>
{
    public QuizAttemptWithQuestionsDtoValidator()
    {
        Include(new QuizAttemptDtoValidator()); // Kế thừa rules từ QuizAttemptDto

        RuleFor(x => x.QuizSections)
            .NotNull()
            .WithMessage("QuizSections không được null")
            .NotEmpty()
            .WithMessage("QuizSections không được rỗng");
    }
}
