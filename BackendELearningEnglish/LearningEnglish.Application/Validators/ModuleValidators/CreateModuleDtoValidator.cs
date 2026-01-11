using FluentValidation;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Validators.ModuleValidators
{
    public class CreateModuleDtoValidator : AbstractValidator<CreateModuleDto>
    {
        public CreateModuleDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên module không được để trống")
                .MaximumLength(200).WithMessage("Tên module không được vượt quá 200 ký tự");

            RuleFor(x => x.Description)
                .MaximumLength(200).WithMessage("Mô tả module không được vượt quá 200 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.ContentType)
                .IsInEnum().WithMessage("Loại nội dung không hợp lệ");

            RuleFor(x => x.OrderIndex)
                .GreaterThanOrEqualTo(0).WithMessage("Thứ tự module phải lớn hơn hoặc bằng 0");

            RuleFor(x => x.LessonId)
                .GreaterThan(0).WithMessage("ID lesson không hợp lệ");
        }
    }
}
