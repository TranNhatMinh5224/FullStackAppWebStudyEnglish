using FluentValidation;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Validators.ModuleValidators
{
    public class UpdateModuleDtoValidator : AbstractValidator<UpdateModuleDto>
    {
        public UpdateModuleDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên module không được để trống")
                .MaximumLength(200).WithMessage("Tên module không được vượt quá 200 ký tự")
                .When(x => x.Name != null);

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Mô tả module không được vượt quá 1000 ký tự")
                .When(x => x.Description != null);

            RuleFor(x => x.ContentType)
                .IsInEnum().WithMessage("Loại nội dung không hợp lệ")
                .When(x => x.ContentType.HasValue);

            RuleFor(x => x.OrderIndex)
                .GreaterThanOrEqualTo(0).WithMessage("Thứ tự module phải lớn hơn hoặc bằng 0")
                .When(x => x.OrderIndex.HasValue);
        }
    }
}
