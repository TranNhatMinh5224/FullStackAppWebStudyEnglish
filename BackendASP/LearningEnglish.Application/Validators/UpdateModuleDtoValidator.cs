using FluentValidation;
using LearningEnglish.Application.DTOS;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Validators
{
    public class UpdateModuleDtoValidator : AbstractValidator<UpdateModuleDto>
    {
        public UpdateModuleDtoValidator()
        {
            // Validate Name - nếu có, phải từ 2-255 ký tự và matching pattern
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên module không được để trống")
                .Length(2, 255)
                .WithMessage("Tên module phải từ 2 đến 255 ký tự")
                .Matches(@"^[\p{L}\p{N}\s\-_.,()]+$")
                .WithMessage("Tên module chỉ được chứa chữ cái, số, khoảng trắng và các ký tự đặc biệt cơ bản")
                .When(x => !string.IsNullOrEmpty(x.Name));

            // Validate Description - nếu có, tối đa 2000 ký tự
            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Mô tả không được vượt quá 2000 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Description));

            // Validate OrderIndex - nếu có, phải >= 0 và <= 999
            RuleFor(x => x.OrderIndex)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Thứ tự phải từ 0 trở lên")
                .LessThan(1000)
                .WithMessage("Thứ tự không được vượt quá 999")
                .When(x => x.OrderIndex.HasValue);

            // Validate ContentType - nếu có, phải là enum hợp lệ
            RuleFor(x => x.ContentType)
                .IsInEnum()
                .WithMessage("Loại nội dung không hợp lệ. Chỉ chấp nhận: Lecture, Quiz, Assignment, FlashCard, Video, Reading")
                .When(x => x.ContentType.HasValue);

            // Đảm bảo ít nhất một field được cập nhật
            RuleFor(x => x)
                .Must(x => !string.IsNullOrEmpty(x.Name) || 
                          !string.IsNullOrEmpty(x.Description) || 
                          x.OrderIndex.HasValue || 
                          x.ContentType.HasValue)
                .WithMessage("Phải có ít nhất một trường được cập nhật");
        }
    }
}
