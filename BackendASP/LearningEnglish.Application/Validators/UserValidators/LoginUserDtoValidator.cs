using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.User
{
    public class LoginUserDtoValidator : AbstractValidator<LoginUserDto>
    {
        public LoginUserDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}
