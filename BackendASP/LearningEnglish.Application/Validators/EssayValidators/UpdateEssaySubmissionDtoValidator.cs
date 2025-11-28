using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.EssayValidators
{
    public class UpdateEssaySubmissionDtoValidator : AbstractValidator<UpdateEssaySubmissionDto>
    {
        public UpdateEssaySubmissionDtoValidator()
        {
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