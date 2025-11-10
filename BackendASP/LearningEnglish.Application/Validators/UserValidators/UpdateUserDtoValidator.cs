using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.User
{
    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.PhoneNumber).NotEmpty();
        }
    }
}
