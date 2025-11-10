using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.TeacherSubscriptionValidators
{
    public class DeleteTeacherSubscriptionDtoValidator : AbstractValidator<DeleteTeacherSubscriptionDto>
    {
        public DeleteTeacherSubscriptionDtoValidator()
        {
            RuleFor(x => x.TeacherSubscriptionId)
                .GreaterThan(0).WithMessage("Teacher subscription ID must be greater than 0");
        }
    }
}
