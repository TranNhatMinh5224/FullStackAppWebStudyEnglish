using FluentValidation;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Validators
{
    public class CreateLectureDtoValidator : AbstractValidator<CreateLectureDto>
    {
        public CreateLectureDtoValidator()
        {
            // Validate ModuleId - bắt buộc và phải > 0
            RuleFor(x => x.ModuleId)
                .NotEmpty()
                .WithMessage("Module ID là bắt buộc")
                .GreaterThan(0)
                .WithMessage("Module ID phải lớn hơn 0");

            // Validate Title - bắt buộc, độ dài từ 2-255 ký tự
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Tiêu đề lecture không được để trống")
                .Length(2, 255)
                .WithMessage("Tiêu đề lecture phải từ 2 đến 255 ký tự")
                .Matches(@"^[\p{L}\p{N}\s\-_.,()!?]+$")
                .WithMessage("Tiêu đề lecture chỉ được chứa chữ cái, số, khoảng trắng và các ký tự đặc biệt cơ bản");

            // Validate OrderIndex - phải >= 0 và <= 999
            RuleFor(x => x.OrderIndex)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Thứ tự phải từ 0 trở lên")
                .LessThan(1000)
                .WithMessage("Thứ tự không được vượt quá 999");

            // Validate NumberingLabel - tùy chọn, tối đa 20 ký tự
            RuleFor(x => x.NumberingLabel)
                .MaximumLength(20)
                .WithMessage("Nhãn đánh số không được vượt quá 20 ký tự")
                .Matches(@"^[0-9a-zA-Z\-._]*$")
                .WithMessage("Nhãn đánh số chỉ được chứa chữ, số và các ký tự -, ., _")
                .When(x => !string.IsNullOrEmpty(x.NumberingLabel));

            // Validate Type - phải là enum hợp lệ
            RuleFor(x => x.Type)
                .NotNull()
                .WithMessage("Loại lecture là bắt buộc")
                .IsInEnum()
                .WithMessage("Loại lecture không hợp lệ. Chỉ chấp nhận: Content, Video, Audio, Document, Interactive");

            // Validate MarkdownContent - tùy chọn, tối đa 50000 ký tự
            RuleFor(x => x.MarkdownContent)
                .MaximumLength(50000)
                .WithMessage("Nội dung Markdown không được vượt quá 50000 ký tự")
                .When(x => !string.IsNullOrEmpty(x.MarkdownContent));

            // Validate ParentLectureId - nếu có phải > 0
            RuleFor(x => x.ParentLectureId)
                .GreaterThan(0)
                .WithMessage("Parent Lecture ID phải lớn hơn 0")
                .When(x => x.ParentLectureId.HasValue);

            // Validate MediaUrl - tùy chọn, tối đa 1000 ký tự
            RuleFor(x => x.MediaUrl)
                .MaximumLength(1000)
                .WithMessage("Media URL không được vượt quá 1000 ký tự")
                .When(x => !string.IsNullOrEmpty(x.MediaUrl));

            // Validate MediaType - tùy chọn, tối đa 50 ký tự
            RuleFor(x => x.MediaType)
                .MaximumLength(50)
                .WithMessage("Media type không được vượt quá 50 ký tự")
                .When(x => !string.IsNullOrEmpty(x.MediaType));

            // Validate MediaSize - nếu có phải > 0
            RuleFor(x => x.MediaSize)
                .GreaterThan(0)
                .WithMessage("Media size phải lớn hơn 0")
                .When(x => x.MediaSize.HasValue);

            // Validate Duration - nếu có phải >= 0
            RuleFor(x => x.Duration)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Duration phải từ 0 trở lên")
                .When(x => x.Duration.HasValue);
        }
    }
}
