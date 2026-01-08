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

            RuleFor(x => x.AudioTempKey)
                .MaximumLength(500).WithMessage("Audio temp key không được quá 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.AudioTempKey));

            RuleFor(x => x.AudioType)
                .MaximumLength(50).WithMessage("Audio type không được quá 50 ký tự")
                .When(x => !string.IsNullOrEmpty(x.AudioType));

            RuleFor(x => x.ImageTempKey)
                .MaximumLength(500).WithMessage("Image temp key không được quá 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.ImageTempKey));

            RuleFor(x => x.ImageType)
                .MaximumLength(50).WithMessage("Image type không được quá 50 ký tự")
                .When(x => !string.IsNullOrEmpty(x.ImageType));
        }
    }
}