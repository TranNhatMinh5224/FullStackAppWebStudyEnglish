using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.EssayValidators
{
    public class UpdateEssaySubmissionDtoValidator : AbstractValidator<UpdateEssaySubmissionDto>
    {
        public UpdateEssaySubmissionDtoValidator()
        {
            RuleFor(x => x.TextContent)
                .NotEmpty().WithMessage("Nội dung bài làm không được để trống")
                .MinimumLength(50).WithMessage("Nội dung bài làm phải có ít nhất 50 ký tự")
                .MaximumLength(10000).WithMessage("Nội dung bài làm không được quá 10000 ký tự");
        }
    }
}