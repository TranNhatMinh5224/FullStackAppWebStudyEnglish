using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators
{
    public class CreateQuizGroupDtoValidator : AbstractValidator<CreateQuizGroupDto>
    {
        public CreateQuizGroupDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên nhóm câu hỏi không được để trống.")
                .MaximumLength(200)
                .WithMessage("Tên nhóm câu hỏi không được vượt quá 200 ký tự.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được vượt quá 1000 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.QuizSectionId)
                .GreaterThan(0)
                .WithMessage("QuizSectionId phải lớn hơn 0.");

            RuleFor(x => x.SumScore)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Tổng điểm phải lớn hơn hoặc bằng 0.");
        }
    }

    public class UpdateQuizGroupDtoValidator : AbstractValidator<UpdateQuizGroupDto>
    {
        public UpdateQuizGroupDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên nhóm câu hỏi không được để trống.")
                .MaximumLength(200)
                .WithMessage("Tên nhóm câu hỏi không được vượt quá 200 ký tự.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được vượt quá 1000 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.SumScore)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Tổng điểm phải lớn hơn hoặc bằng 0.");
        }
    }
}
