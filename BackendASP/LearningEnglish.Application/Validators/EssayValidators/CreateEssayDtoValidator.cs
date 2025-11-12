using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.EssayValidators
{
    public class CreateEssayDtoValidator : AbstractValidator<CreateEssayDto>
    {
        public CreateEssayDtoValidator()
        {
            RuleFor(x => x.AssessmentId)
                .GreaterThan(0).WithMessage("Assessment ID phải lớn hơn 0");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Tiêu đề Essay không được để trống")
                .MaximumLength(200).WithMessage("Tiêu đề Essay không được quá 200 ký tự");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Mô tả Essay không được quá 2000 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}