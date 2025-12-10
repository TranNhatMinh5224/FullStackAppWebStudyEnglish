using FluentValidation;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Validators
{
    public class BulkCreateLecturesDtoValidator : AbstractValidator<BulkCreateLecturesDto>
    {
        public BulkCreateLecturesDtoValidator()
        {
            // Validate ModuleId
            RuleFor(x => x.ModuleId)
                .NotEmpty()
                .WithMessage("Module ID là bắt buộc")
                .GreaterThan(0)
                .WithMessage("Module ID phải lớn hơn 0");

            // Validate Lectures list
            RuleFor(x => x.Lectures)
                .NotEmpty()
                .WithMessage("Danh sách lectures không được để trống")
                .Must(x => x.Count <= 100)
                .WithMessage("Không thể tạo quá 100 lectures cùng lúc");

            // Validate each lecture node
            RuleForEach(x => x.Lectures)
                .SetValidator(new LectureNodeDtoValidator());
        }
    }

    public class LectureNodeDtoValidator : AbstractValidator<LectureNodeDto>
    {
        public LectureNodeDtoValidator()
        {
            // Validate TempId - bắt buộc
            RuleFor(x => x.TempId)
                .NotEmpty()
                .WithMessage("TempId là bắt buộc")
                .Matches(@"^[a-zA-Z0-9\-_]+$")
                .WithMessage("TempId chỉ được chứa chữ, số, dấu gạch ngang và gạch dưới");

            // Validate ParentTempId - nếu có thì phải khác TempId
            RuleFor(x => x.ParentTempId)
                .Must((node, parentTempId) => string.IsNullOrEmpty(parentTempId) || parentTempId != node.TempId)
                .WithMessage("ParentTempId không thể trùng với TempId (lecture không thể là cha của chính nó)");

            // Validate Title - bắt buộc
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Tiêu đề lecture không được để trống")
                .Length(2, 255)
                .WithMessage("Tiêu đề lecture phải từ 2 đến 255 ký tự")
                .Matches(@"^[\p{L}\p{N}\s\-_.,()!?]+$")
                .WithMessage("Tiêu đề lecture chỉ được chứa chữ cái, số, khoảng trắng và các ký tự đặc biệt cơ bản");

            // Validate OrderIndex
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

            // Validate Type
            RuleFor(x => x.Type)
                .NotNull()
                .WithMessage("Loại lecture là bắt buộc")
                .IsInEnum()
                .WithMessage("Loại lecture không hợp lệ");

            // Validate MarkdownContent - tùy chọn
            RuleFor(x => x.MarkdownContent)
                .MaximumLength(50000)
                .WithMessage("Nội dung Markdown không được vượt quá 50000 ký tự")
                .When(x => !string.IsNullOrEmpty(x.MarkdownContent));

            // Validate MediaType - nếu có
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
