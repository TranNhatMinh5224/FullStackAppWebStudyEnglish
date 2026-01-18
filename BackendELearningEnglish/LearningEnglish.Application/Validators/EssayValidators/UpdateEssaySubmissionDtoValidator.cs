using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.EssayValidators
{
    public class UpdateEssaySubmissionDtoValidator : AbstractValidator<UpdateEssaySubmissionDto>
    {
        public UpdateEssaySubmissionDtoValidator()
        {
            // Cho phép nộp bài trống - không yêu cầu minimum length
            // Chỉ validate maximum length nếu có TextContent
            RuleFor(x => x.TextContent)
                .MaximumLength(1000000).WithMessage("Nội dung bài làm không được quá 1,000,000 ký tự")
                .When(x => !string.IsNullOrWhiteSpace(x.TextContent));

            // Validate file type nếu có AttachmentTempKey
            RuleFor(x => x.AttachmentType)
                .NotEmpty()
                .WithMessage("Phải chỉ định loại file đính kèm")
                .Must(type => type == "application/pdf" || 
                             type == "application/vnd.openxmlformats-officedocument.wordprocessingml.document" ||
                             type == "application/msword" ||
                             type == "text/plain" ||
                             type == "application/vnd.ms-word.document.macroEnabled.12" ||
                             type == "application/vnd.openxmlformats-officedocument.wordprocessingml.template" ||
                             type == "application/vnd.ms-word.template.macroEnabled.12")
                .WithMessage("Chỉ chấp nhận file PDF, Word (.doc, .docx), hoặc Text (.txt)")
                .When(x => !string.IsNullOrWhiteSpace(x.AttachmentTempKey));
        }
    }
}