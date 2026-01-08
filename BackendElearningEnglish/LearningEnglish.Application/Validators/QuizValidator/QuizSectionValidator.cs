using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators
{
    public class CreateQuizSectionDtoValidator : AbstractValidator<CreateQuizSectionDto>
    {
        public CreateQuizSectionDtoValidator()
        {
            RuleFor(x => x.QuizId)
                .GreaterThan(0)
                .WithMessage("QuizId phải lớn hơn 0.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Tiêu đề không được để trống.")
                .MaximumLength(200)
                .WithMessage("Tiêu đề không được vượt quá 200 ký tự.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được vượt quá 1000 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }

    public class UpdateQuizSectionDtoValidator : AbstractValidator<UpdateQuizSectionDto>
    {
        public UpdateQuizSectionDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Tiêu đề không được để trống.")
                .MaximumLength(200)
                .WithMessage("Tiêu đề không được vượt quá 200 ký tự.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được vượt quá 1000 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}
