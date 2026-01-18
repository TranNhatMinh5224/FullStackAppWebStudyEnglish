using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.PronunciationAssessmentValidators
{
    public class CreatePronunciationAssessmentDtoValidator : AbstractValidator<CreatePronunciationAssessmentDto>
    {
        public CreatePronunciationAssessmentDtoValidator()
        {
            // Không có validation rules
        }
    }
}
