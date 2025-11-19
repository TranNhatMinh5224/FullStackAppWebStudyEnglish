using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;
using FluentValidation;
using LearningEnglish.Application.Interface;

namespace LearningEnglish.Application.Validators.CourseValidators
{
    public class TeacherCreateCourseRequestDtoValidator : AbstractValidator<TeacherCreateCourseRequestDto>
    {
        private readonly ICourseRepository _courseRepository;

        public TeacherCreateCourseRequestDtoValidator(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Course title is required")
                .MaximumLength(200).WithMessage("Course title must not exceed 200 characters")
                .Must((dto, title) =>
                {
                    if (string.IsNullOrWhiteSpace(title)) return true;
                    // No direct teacherId in validator, so skip uniqueness here; service will handle per-teacher uniqueness.
                    return true;
                }).WithMessage("Course title validation failed");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Course description is required")
                .MaximumLength(2000).WithMessage("Course description must not exceed 2000 characters");

            RuleFor(x => x.Img)
                .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Img));

            RuleFor(x => x.ImageType)
                .MaximumLength(50).WithMessage("Image type must not exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.ImageType));

            RuleFor(x => x.MaxStudent)
                .GreaterThanOrEqualTo(0).WithMessage("MaxStudent must be greater than or equal to 0 (0 means unlimited)");

            // Teacher courses không có price (miễn phí)
            RuleFor(x => x.Type)
                .Equal(CourseType.Teacher).WithMessage("Teacher can only create Teacher type courses");
        }
    }
}
