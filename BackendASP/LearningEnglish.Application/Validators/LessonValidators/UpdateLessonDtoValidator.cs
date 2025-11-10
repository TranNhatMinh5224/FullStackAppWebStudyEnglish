using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.LessonValidators
{
    public class UpdateLessonDtoValidator : AbstractValidator<UpdateLessonDto>
    {
        public UpdateLessonDtoValidator()
        {

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Lesson title is required")
                .MaximumLength(200).WithMessage("Lesson title must not exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Lesson description is required")
                .MaximumLength(2000).WithMessage("Lesson description must not exceed 2000 characters");


        }
    }
}
