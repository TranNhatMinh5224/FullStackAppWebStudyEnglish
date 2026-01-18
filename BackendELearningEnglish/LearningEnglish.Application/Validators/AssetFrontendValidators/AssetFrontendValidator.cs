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

         
            
        }
    }

    public class UpdateAssetFrontendDtoValidator : AbstractValidator<UpdateAssetFrontendDto>
    {
        public UpdateAssetFrontendDtoValidator()
        {
            // Id có thể đến từ route parameter (controller sẽ set) hoặc từ body
            // Validate Id nếu có giá trị, nhưng không bắt buộc vì controller sẽ set từ route
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("ID phải lớn hơn 0")
                .When(x => x.Id > 0); // Chỉ validate khi có giá trị

            RuleFor(x => x.NameImage)
                .MaximumLength(200).WithMessage("Tên hình ảnh không được vượt quá 200 ký tự")
                .When(x => x.NameImage != null && !string.IsNullOrWhiteSpace(x.NameImage));

            RuleFor(x => x.AssetType)
                .IsInEnum().WithMessage("Loại Asset không hợp lệ")
                .When(x => x.AssetType.HasValue);
        }
    }
}
