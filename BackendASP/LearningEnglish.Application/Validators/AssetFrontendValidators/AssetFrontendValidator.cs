using FluentValidation;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Validators.AssetFrontendValidators
{
    public class CreateAssetFrontendDtoValidator : AbstractValidator<CreateAssetFrontendDto>
    {
        public CreateAssetFrontendDtoValidator()
        {
            RuleFor(x => x.NameImage)
                .NotEmpty().WithMessage("Tên hình ảnh không được để trống")
                .MaximumLength(200).WithMessage("Tên hình ảnh không được vượt quá 200 ký tự");

            RuleFor(x => x.DescriptionImage)
                .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự");

            RuleFor(x => x.AssetType)
                .IsInEnum().WithMessage("Loại Asset không hợp lệ");

            RuleFor(x => x.Order)
                .GreaterThanOrEqualTo(0).WithMessage("Thứ tự hiển thị không được nhỏ hơn 0");

            RuleFor(x => x.ImageTempKey)
                .NotEmpty().WithMessage("Vui lòng tải lên hình ảnh");
        }
    }

    public class UpdateAssetFrontendDtoValidator : AbstractValidator<UpdateAssetFrontendDto>
    {
        public UpdateAssetFrontendDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID không được để trống");

            RuleFor(x => x.NameImage)
                .MaximumLength(200).WithMessage("Tên hình ảnh không được vượt quá 200 ký tự")
                .When(x => x.NameImage != null);

            RuleFor(x => x.DescriptionImage)
                .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự")
                .When(x => x.DescriptionImage != null);

            RuleFor(x => x.AssetType)
                .IsInEnum().WithMessage("Loại Asset không hợp lệ")
                .When(x => x.AssetType.HasValue);

            RuleFor(x => x.Order)
                .GreaterThanOrEqualTo(0).WithMessage("Thứ tự hiển thị không được nhỏ hơn 0")
                .When(x => x.Order.HasValue);
        }
    }
}
