using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.EssayValidators
{
    public class UpdateEssaySubmissionDtoValidator : AbstractValidator<UpdateEssaySubmissionDto>
    {
        public UpdateEssaySubmissionDtoValidator()
        {
            RuleFor(x => x.TextContent)
                .MinimumLength(10).WithMessage("Nội dung bài làm phải có ít nhất 10 ký tự")
                .MaximumLength(10000).WithMessage("Nội dung bài làm không được quá 10000 ký tự")
                .When(x => !string.IsNullOrWhiteSpace(x.TextContent));

           
        }
    }
}