using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.EssayValidators
{
    public class UpdateEssayDtoValidator : AbstractValidator<UpdateEssayDto>
    {
        public UpdateEssayDtoValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(200).WithMessage("Tiêu đề Essay không được quá 200 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Title));

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Mô tả Essay không được quá 2000 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.TotalPoints)
                .GreaterThan(0).WithMessage("Điểm tối đa phải lớn hơn 0")
                
                .When(x => x.TotalPoints.HasValue);

            
        }
    }
}