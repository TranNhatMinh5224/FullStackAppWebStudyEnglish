using FluentValidation;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Validators
{
    public class UpdateLectureDtoValidator : AbstractValidator<UpdateLectureDto>
    {
        public UpdateLectureDtoValidator()
        {
            // Validate Title - nếu có, phải từ 2-255 ký tự và matching pattern
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Tiêu đề lecture không được để trống")
                .Length(2, 255)
                .WithMessage("Tiêu đề lecture phải từ 2 đến 255 ký tự")
                .Matches(@"^[\p{L}\p{N}\s\-_.,()!?]+$")
                .WithMessage("Tiêu đề lecture chỉ được chứa chữ cái, số, khoảng trắng và các ký tự đặc biệt cơ bản")
                .When(x => !string.IsNullOrEmpty(x.Title));

            // Validate OrderIndex - nếu có, phải >= 0 và <= 999
            RuleFor(x => x.OrderIndex)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Thứ tự phải từ 0 trở lên")
                .LessThan(1000)
                .WithMessage("Thứ tự không được vượt quá 999")
                .When(x => x.OrderIndex.HasValue);

            // Validate NumberingLabel - nếu có (hỗ trợ tiếng Việt)
            RuleFor(x => x.NumberingLabel)
                .MaximumLength(50)
                .WithMessage("Nhãn đánh số không được vượt quá 50 ký tự")
                .Matches(@"^[\p{L}\p{N}\s\-._]*$")
                .WithMessage("Nhãn đánh số chỉ được chứa chữ, số, khoảng trắng và các ký tự -, ., _")
                .When(x => !string.IsNullOrEmpty(x.NumberingLabel));

            // Validate Type - nếu có, phải là enum hợp lệ
            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Loại lecture không hợp lệ. Chỉ chấp nhận: Content, Video, Audio, Document, Interactive")
                .When(x => x.Type.HasValue);

            // Validate MarkdownContent - nếu có, tối đa 50000 ký tự
            RuleFor(x => x.MarkdownContent)
                .MaximumLength(50000)
                .WithMessage("Nội dung Markdown không được vượt quá 50000 ký tự")
                .When(x => !string.IsNullOrEmpty(x.MarkdownContent));

            // Validate ParentLectureId - nếu có phải > 0
            RuleFor(x => x.ParentLectureId)
                .GreaterThan(0)
                .WithMessage("Parent Lecture ID phải lớn hơn 0")
                .When(x => x.ParentLectureId.HasValue);

            // Validate MediaType - nếu có, tối đa 50 ký tự
           

            // Validate Duration - nếu có phải >= 0
            RuleFor(x => x.Duration)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Duration phải từ 0 trở lên")
                .When(x => x.Duration.HasValue);

            // Đảm bảo ít nhất một field được cập nhật
            RuleFor(x => x)
                .Must(x => !string.IsNullOrEmpty(x.Title) ||
                          x.OrderIndex.HasValue ||
                          !string.IsNullOrEmpty(x.NumberingLabel) ||
                          x.Type.HasValue ||
                          !string.IsNullOrEmpty(x.MarkdownContent) ||
                          x.ParentLectureId.HasValue ||
                          !string.IsNullOrEmpty(x.MediaTempKey) ||
                          !string.IsNullOrEmpty(x.MediaType) ||
                          x.MediaSize.HasValue ||
                          x.Duration.HasValue)
                .WithMessage("Phải có ít nhất một trường được cập nhật");
        }
    }
}
