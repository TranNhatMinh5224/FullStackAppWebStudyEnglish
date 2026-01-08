using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.CourseValidators
{
    // ✅ Validator cho PARTIAL UPDATE - Chỉ validate các trường có giá trị (nullable)
    public class AdminUpdateCourseRequestDtoValidator : AbstractValidator<AdminUpdateCourseRequestDto>
    {
        public AdminUpdateCourseRequestDtoValidator()
        {
            // Title: Chỉ validate khi có giá trị
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Course title cannot be empty if provided")
                .MaximumLength(200).WithMessage("Course title must not exceed 200 characters")
                .When(x => x.Title != null); // Chỉ validate khi Title được gửi

            // Description: Chỉ validate khi có giá trị
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Course description cannot be empty if provided")
                .MaximumLength(2000).WithMessage("Course description must not exceed 2000 characters")
                .When(x => x.Description != null);

            // Price: Chỉ validate khi có giá trị
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0")
                .When(x => x.Price.HasValue);

            // MaxStudent: Chỉ validate khi có giá trị
            RuleFor(x => x.MaxStudent)
                .GreaterThan(0).WithMessage("MaxStudent must be greater than 0 if provided")
                .When(x => x.MaxStudent.HasValue);
        }
    }
}
