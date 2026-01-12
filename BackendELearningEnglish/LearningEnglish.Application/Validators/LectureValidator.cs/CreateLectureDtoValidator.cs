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
                .Matches(@"^[\p{L}\p{N}\s\-_.,()!?:&\[\]]+$")
                .WithMessage("Tiêu đề lecture chỉ được chứa chữ cái, số, khoảng trắng và các ký tự đặc biệt cơ bản");

            // Validate OrderIndex - phải >= 0 và <= 999
            RuleFor(x => x.OrderIndex)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Thứ tự phải từ 0 trở lên")
                .LessThan(1000)
                .WithMessage("Thứ tự không được vượt quá 999");

            // Validate NumberingLabel - tùy chọn (hỗ trợ tiếng Việt)
            RuleFor(x => x.NumberingLabel)
                .MaximumLength(50)
                .WithMessage("Nhãn đánh số không được vượt quá 50 ký tự")
                .Matches(@"^[\p{L}\p{N}\s\-._]*$")
                .WithMessage("Nhãn đánh số chỉ được chứa chữ, số, khoảng trắng và các ký tự -, ., _")
                .When(x => !string.IsNullOrEmpty(x.NumberingLabel));

            // Validate Type - phải là enum hợp lệ (Content, Document, Video)
            RuleFor(x => x.Type)
                .NotNull()
                .WithMessage("Loại lecture là bắt buộc")
                .IsInEnum()
                .WithMessage("Loại lecture không hợp lệ. Chỉ chấp nhận: Content (1), Document (2), Video (3)");

            // Validate MarkdownContent - tùy chọn, tối đa 5 triệu ký tự (chỉ cho Document)
            RuleFor(x => x.MarkdownContent)
                .MaximumLength(5000000)
                .WithMessage("Nội dung Markdown không được vượt quá 5 triệu ký tự")
                .When(x => !string.IsNullOrEmpty(x.MarkdownContent));

            // Validate ParentLectureId - nếu có phải > 0
            RuleFor(x => x.ParentLectureId)
                .GreaterThan(0)
                .WithMessage("Parent Lecture ID phải lớn hơn 0")
                .When(x => x.ParentLectureId.HasValue);

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
