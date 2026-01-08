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
                .MinimumLength(10).WithMessage("Nội dung bài làm phải có ít nhất 10 ký tự")
                .MaximumLength(1000000).WithMessage("Nội dung bài làm không được quá 10000 ký tự")
                .When(x => !string.IsNullOrWhiteSpace(x.TextContent));

         
        }
    }
}