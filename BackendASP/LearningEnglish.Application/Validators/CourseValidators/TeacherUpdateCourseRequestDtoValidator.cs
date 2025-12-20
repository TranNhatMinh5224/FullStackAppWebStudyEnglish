using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;
using FluentValidation;

namespace LearningEnglish.Application.Validators.CourseValidators
{
    public class TeacherUpdateCourseRequestDtoValidator : AbstractValidator<TeacherUpdateCourseRequestDto>
    {
        public TeacherUpdateCourseRequestDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Course title is required")
                .MaximumLength(200).WithMessage("Course title must not exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Course description is required")
                .MaximumLength(2000).WithMessage("Course description must not exceed 2000 characters");

            

            RuleFor(x => x.MaxStudent)
                .GreaterThanOrEqualTo(0).WithMessage("MaxStudent must be greater than or equal to 0 (0 means unlimited)");

            // Teacher courses không có price (miễn phí)
            RuleFor(x => x.Type)
                .Equal(CourseType.Teacher).WithMessage("Teacher can only update Teacher type courses");
        }
    }
}
