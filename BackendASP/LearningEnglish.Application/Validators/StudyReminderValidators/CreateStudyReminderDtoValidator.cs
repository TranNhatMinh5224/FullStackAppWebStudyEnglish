using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.StudyReminderValidators
{
    public class CreateStudyReminderDtoValidator : AbstractValidator<CreateStudyReminderDto>
    {
        public CreateStudyReminderDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("UserId must be provided");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title must be at most 200 characters");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message is required")
                .MaximumLength(500).WithMessage("Message must be at most 500 characters");

            RuleFor(x => x.ScheduledTime)
                .NotEmpty().WithMessage("ScheduledTime is required")
                .Matches("^([01]\\d|2[0-3]):([0-5]\\d)$").WithMessage("ScheduledTime must be in HH:mm format");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Type is invalid");
        }
    }
}
