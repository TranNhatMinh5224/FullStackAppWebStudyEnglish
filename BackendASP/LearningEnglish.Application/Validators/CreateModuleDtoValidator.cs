using FluentValidation;
using LearningEnglish.Application.DTOS;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Validators
{
    public class CreateModuleDtoValidator : AbstractValidator<CreateModuleDto>
    {
        public CreateModuleDtoValidator()
        {
            // Validate LessonId - bắt buộc và phải > 0
            RuleFor(x => x.LessonId)
                .NotEmpty()
                .WithMessage("Lesson ID là bắt buộc")
                .GreaterThan(0)
                .WithMessage("Lesson ID phải lớn hơn 0");

            // Validate Name - bắt buộc, độ dài từ 2-255 ký tự
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên module không được để trống")
                .Length(2, 255)
                .WithMessage("Tên module phải từ 2 đến 255 ký tự")
                .Matches(@"^[\p{L}\p{N}\s\-_.,()]+$")
                .WithMessage("Tên module chỉ được chứa chữ cái, số, khoảng trắng và các ký tự đặc biệt cơ bản");

            // Validate Description - tùy chọn, tối đa 2000 ký tự
            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Mô tả không được vượt quá 2000 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Description));

            // Validate OrderIndex - phải >= 0 và <= 999
            RuleFor(x => x.OrderIndex)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Thứ tự phải từ 0 trở lên")
                .LessThan(1000)
                .WithMessage("Thứ tự không được vượt quá 999");

            // Validate ContentType - phải là enum hợp lệ
            RuleFor(x => x.ContentType)
                .NotNull()
                .WithMessage("Loại nội dung là bắt buộc")
                .IsInEnum()
                .WithMessage("Loại nội dung không hợp lệ. Chỉ chấp nhận: Lecture, Quiz, Assignment, FlashCard, Video, Reading");
        }
    }
}
