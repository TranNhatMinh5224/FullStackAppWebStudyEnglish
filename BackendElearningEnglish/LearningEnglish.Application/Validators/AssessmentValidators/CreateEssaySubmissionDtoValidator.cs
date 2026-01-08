using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.AssessmentValidators;

public class CreateEssaySubmissionDtoValidator : AbstractValidator<CreateEssaySubmissionDto>
{
    public CreateEssaySubmissionDtoValidator()
    {
        RuleFor(x => x.EssayId)
            .GreaterThan(0)
            .WithMessage("EssayId phải lớn hơn 0");

        // Phải có ít nhất TextContent HOẶC AttachmentTempKey
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.TextContent) || !string.IsNullOrWhiteSpace(x.AttachmentTempKey))
            .WithMessage("Bài làm phải có nội dung text hoặc file đính kèm");

        // TextContent validation (nếu có)
        When(x => !string.IsNullOrWhiteSpace(x.TextContent), () =>
        {
            RuleFor(x => x.TextContent)
                .MinimumLength(50)
                .WithMessage("Nội dung bài làm phải có ít nhất 50 ký tự")
                .MaximumLength(50000)
                .WithMessage("Nội dung bài làm không được vượt quá 50,000 ký tự");
        });

        // Attachment validation (nếu có)
        When(x => !string.IsNullOrWhiteSpace(x.AttachmentTempKey), () =>
        {
            RuleFor(x => x.AttachmentType)
                .NotEmpty()
                .WithMessage("Phải chỉ định loại file đính kèm")
                .Must(type => type == "application/pdf" || 
                             type == "application/vnd.openxmlformats-officedocument.wordprocessingml.document" ||
                             type == "application/msword")
                .WithMessage("Chỉ chấp nhận file PDF hoặc Word (.doc, .docx)");
        });
    }
}
