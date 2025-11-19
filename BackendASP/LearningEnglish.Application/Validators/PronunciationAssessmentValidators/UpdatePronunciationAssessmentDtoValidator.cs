using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.PronunciationAssessmentValidators
{
    public class UpdatePronunciationAssessmentDtoValidator : AbstractValidator<UpdatePronunciationAssessmentDto>
    {
        public UpdatePronunciationAssessmentDtoValidator()
        {
            // Validate ReferenceText - nếu có, tối đa 500 ký tự
            RuleFor(x => x.ReferenceText)
                .MaximumLength(500)
                .WithMessage("Reference text must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.ReferenceText));

            // Validate AudioUrl - nếu có, tối đa 1000 ký tự
            RuleFor(x => x.AudioUrl)
                .MaximumLength(1000)
                .WithMessage("Audio URL must not exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.AudioUrl));

            // Validate AudioType - nếu có, tối đa 50 ký tự
            RuleFor(x => x.AudioType)
                .MaximumLength(50)
                .WithMessage("Audio type must not exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.AudioType));

            // Validate AudioSize - nếu có phải > 0
            RuleFor(x => x.AudioSize)
                .GreaterThan(0)
                .WithMessage("Audio size must be greater than 0")
                .When(x => x.AudioSize.HasValue);

            // Validate OverallScore - nếu có phải từ 0-100
            RuleFor(x => x.OverallScore)
                .InclusiveBetween(0, 100)
                .WithMessage("Overall score must be between 0 and 100")
                .When(x => x.OverallScore.HasValue);

            // Validate Feedback - nếu có, tối đa 1000 ký tự
            RuleFor(x => x.Feedback)
                .MaximumLength(1000)
                .WithMessage("Feedback must not exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.Feedback));

            // Đảm bảo ít nhất một field được cập nhật
            RuleFor(x => x)
                .Must(x => !string.IsNullOrEmpty(x.ReferenceText) ||
                          !string.IsNullOrEmpty(x.AudioUrl) ||
                          !string.IsNullOrEmpty(x.AudioType) ||
                          x.AudioSize.HasValue ||
                          x.OverallScore.HasValue ||
                          !string.IsNullOrEmpty(x.Feedback))
                .WithMessage("At least one field must be updated");
        }
    }
}

