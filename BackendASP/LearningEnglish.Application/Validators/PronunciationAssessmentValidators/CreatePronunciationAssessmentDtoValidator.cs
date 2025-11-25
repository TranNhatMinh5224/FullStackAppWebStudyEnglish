using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.PronunciationAssessmentValidators
{
    public class CreatePronunciationAssessmentDtoValidator : AbstractValidator<CreatePronunciationAssessmentDto>
    {
        public CreatePronunciationAssessmentDtoValidator()
        {
            // Validate ReferenceText - bắt buộc
            RuleFor(x => x.ReferenceText)
                .NotEmpty()
                .WithMessage("Reference text is required")
                .MaximumLength(500)
                .WithMessage("Reference text must not exceed 500 characters");

            // Validate AudioTempKey - bắt buộc
            RuleFor(x => x.AudioTempKey)
                .NotEmpty()
                .WithMessage("Audio temp key is required")
                .MaximumLength(500)
                .WithMessage("Audio temp key must not exceed 500 characters");

            // Validate AudioType - tùy chọn, tối đa 50 ký tự
            RuleFor(x => x.AudioType)
                .MaximumLength(50)
                .WithMessage("Audio type must not exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.AudioType));

            // Validate AudioSize - nếu có phải > 0
            RuleFor(x => x.AudioSize)
                .GreaterThan(0)
                .WithMessage("Audio size must be greater than 0")
                .When(x => x.AudioSize.HasValue);

            // Validate FlashCardId - nếu có phải > 0
            RuleFor(x => x.FlashCardId)
                .GreaterThan(0)
                .WithMessage("FlashCard ID must be greater than 0")
                .When(x => x.FlashCardId.HasValue);

            // Validate AssignmentId - nếu có phải > 0
            RuleFor(x => x.AssignmentId)
                .GreaterThan(0)
                .WithMessage("Assignment ID must be greater than 0")
                .When(x => x.AssignmentId.HasValue);
        }
    }
}

