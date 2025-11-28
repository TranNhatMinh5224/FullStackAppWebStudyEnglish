using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.EssayValidators
{
    public class CreateEssaySubmissionDtoValidator : AbstractValidator<CreateEssaySubmissionDto>
    {
        public CreateEssaySubmissionDtoValidator()
        {
            RuleFor(x => x.EssayId)
                .GreaterThan(0).WithMessage("Essay ID phải lớn hơn 0");

            // TextContent hoặc AttachmentTempKey phải có ít nhất 1
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.TextContent) || !string.IsNullOrWhiteSpace(x.AttachmentTempKey))
                .WithMessage("Phải có ít nhất nội dung văn bản hoặc file đính kèm");

            RuleFor(x => x.TextContent)
                .MinimumLength(50).WithMessage("Nội dung bài làm phải có ít nhất 50 ký tự")
                .MaximumLength(10000).WithMessage("Nội dung bài làm không được quá 10000 ký tự")
                .When(x => !string.IsNullOrWhiteSpace(x.TextContent));

            RuleFor(x => x.AttachmentTempKey)
                .MaximumLength(500).WithMessage("Attachment key không được quá 500 ký tự")
                .When(x => !string.IsNullOrWhiteSpace(x.AttachmentTempKey));

            RuleFor(x => x.AttachmentType)
                .MaximumLength(50).WithMessage("Attachment type không được quá 50 ký tự")
                .When(x => !string.IsNullOrWhiteSpace(x.AttachmentType));
        }
    }
}