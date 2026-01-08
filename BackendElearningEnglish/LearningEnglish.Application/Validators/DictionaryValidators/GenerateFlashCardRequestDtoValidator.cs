using LearningEnglish.Application.DTOs;
using FluentValidation;

namespace LearningEnglish.Application.Validators.DictionaryValidators
{
    public class GenerateFlashCardRequestDtoValidator : AbstractValidator<GenerateFlashCardRequestDto>
    {
        public GenerateFlashCardRequestDtoValidator()
        {
            RuleFor(x => x.Word)
                .NotEmpty().WithMessage("Word is required")
                .MaximumLength(100).WithMessage("Word must not exceed 100 characters");
        }
    }
}

