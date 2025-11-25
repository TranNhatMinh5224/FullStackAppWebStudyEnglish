using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators
{
    public class CreateFlashCardDtoValidator : AbstractValidator<CreateFlashCardDto>
    {
        public CreateFlashCardDtoValidator()
        {
            RuleFor(x => x.Word)
                .NotEmpty().WithMessage("Từ vựng không được để trống")
                .MaximumLength(100).WithMessage("Từ vựng không được vượt quá 100 ký tự")
                .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("Từ vựng chỉ được chứa chữ cái, khoảng trắng, dấu gạch ngang và dấu nháy")
                .Must(word => !string.IsNullOrWhiteSpace(word) && word.Trim().Length > 0)
                .WithMessage("Từ vựng không được chỉ chứa khoảng trắng");

            RuleFor(x => x.Meaning)
                .NotEmpty().WithMessage("Nghĩa của từ không được để trống")
                .MaximumLength(500).WithMessage("Nghĩa của từ không được vượt quá 500 ký tự");

            RuleFor(x => x.Pronunciation)
                .MaximumLength(200).WithMessage("Phiên âm không được vượt quá 200 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Pronunciation));

            RuleFor(x => x.PartOfSpeech)
                .MaximumLength(50).WithMessage("Từ loại không được vượt quá 50 ký tự")
                .When(x => !string.IsNullOrEmpty(x.PartOfSpeech));

            RuleFor(x => x.Example)
                .MaximumLength(500).WithMessage("Câu ví dụ không được vượt quá 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Example));

            RuleFor(x => x.ExampleTranslation)
                .MaximumLength(500).WithMessage("Dịch câu ví dụ không được vượt quá 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.ExampleTranslation));

            RuleFor(x => x.Synonyms)
                .MaximumLength(500).WithMessage("Từ đồng nghĩa không được vượt quá 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Synonyms));

            RuleFor(x => x.Antonyms)
                .MaximumLength(500).WithMessage("Từ trái nghĩa không được vượt quá 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Antonyms));

            RuleFor(x => x.ImageTempKey)
                .MaximumLength(500).WithMessage("Image temp key không được vượt quá 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.ImageTempKey));

            RuleFor(x => x.AudioTempKey)
                .MaximumLength(500).WithMessage("Audio temp key không được vượt quá 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.AudioTempKey));

            RuleFor(x => x.ModuleId)
                .GreaterThan(0).WithMessage("ID Module phải là số dương")
                .When(x => x.ModuleId.HasValue);
        }
    }

    public class UpdateFlashCardDtoValidator : AbstractValidator<UpdateFlashCardDto>
    {
        public UpdateFlashCardDtoValidator()
        {
            RuleFor(x => x.Word)
                .MaximumLength(100).WithMessage("Từ vựng không được vượt quá 100 ký tự")
                .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("Từ vựng chỉ được chứa chữ cái, khoảng trắng, dấu gạch ngang và dấu nháy")
                .Must(word => string.IsNullOrEmpty(word) || (!string.IsNullOrWhiteSpace(word) && word.Trim().Length > 0))
                .WithMessage("Từ vựng không được chỉ chứa khoảng trắng")
                .When(x => !string.IsNullOrEmpty(x.Word));

            RuleFor(x => x.Meaning)
                .MaximumLength(500).WithMessage("Nghĩa của từ không được vượt quá 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Meaning));

            RuleFor(x => x.Pronunciation)
                .MaximumLength(200).WithMessage("Phiên âm không được vượt quá 200 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Pronunciation));

            RuleFor(x => x.PartOfSpeech)
                .MaximumLength(50).WithMessage("Từ loại không được vượt quá 50 ký tự")
                .When(x => !string.IsNullOrEmpty(x.PartOfSpeech));

            RuleFor(x => x.Example)
                .MaximumLength(500).WithMessage("Câu ví dụ không được vượt quá 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Example));

            RuleFor(x => x.ExampleTranslation)
                .MaximumLength(500).WithMessage("Dịch câu ví dụ không được vượt quá 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.ExampleTranslation));

            RuleFor(x => x.Synonyms)
                .MaximumLength(500).WithMessage("Từ đồng nghĩa không được vượt quá 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Synonyms));

            RuleFor(x => x.Antonyms)
                .MaximumLength(500).WithMessage("Từ trái nghĩa không được vượt quá 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Antonyms));

            RuleFor(x => x.ImageTempKey)
                .MaximumLength(500).WithMessage("Image temp key không được vượt quá 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.ImageTempKey));

            RuleFor(x => x.AudioTempKey)
                .MaximumLength(500).WithMessage("Audio temp key không được vượt quá 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.AudioTempKey));
        }
    }

    public class BulkImportFlashCardDtoValidator : AbstractValidator<BulkImportFlashCardDto>
    {
        public BulkImportFlashCardDtoValidator()
        {
            RuleFor(x => x.ModuleId)
                .GreaterThan(0).WithMessage("ID Module phải là số dương");

            RuleFor(x => x.FlashCards)
                .NotEmpty().WithMessage("Danh sách FlashCard không được để trống")
                .Must(x => x.Count <= 100).WithMessage("Tối đa 100 FlashCard trong một lần import");

            RuleForEach(x => x.FlashCards)
                .SetValidator(new CreateFlashCardDtoValidator());
        }
    }
}
