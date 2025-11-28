using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.User
{
    public class UpdateAvatarDtoValidator : AbstractValidator<UpdateAvatarDto>
    {
        public UpdateAvatarDtoValidator()
        {
            RuleFor(x => x.AvatarTempKey)
                .NotEmpty().WithMessage("Avatar temp key is required")
                .MaximumLength(500).WithMessage("Avatar temp key must not exceed 500 characters");

            RuleFor(x => x.AvatarType)
                .MaximumLength(50).WithMessage("Avatar type must not exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.AvatarType));
        }
    }
}
