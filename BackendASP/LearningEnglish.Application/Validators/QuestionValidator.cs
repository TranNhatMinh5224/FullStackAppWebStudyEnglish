using FluentValidation;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Validators
{
    public class QuestionCreateDtoValidator : AbstractValidator<QuestionCreateDto>
    {
        public QuestionCreateDtoValidator()
        {
            RuleFor(x => x.StemText)
                .NotEmpty()
                .WithMessage("Nội dung câu hỏi không được để trống.")
                .MaximumLength(2000)
                .WithMessage("Nội dung câu hỏi không được vượt quá 2000 ký tự.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Loại câu hỏi không hợp lệ.");

            RuleFor(x => x.QuizGroupId)
                .GreaterThan(0)
                .WithMessage("QuizGroupId phải lớn hơn 0.");

            RuleFor(x => x.QuizSectionId)
                .GreaterThan(0)
                .WithMessage("QuizSectionId phải lớn hơn 0.");

            RuleFor(x => x.Points)
                .GreaterThan(0)
                .WithMessage("Điểm số phải lớn hơn 0.")
                .LessThanOrEqualTo(1000)
                .WithMessage("Điểm số không được vượt quá 1000.");

            RuleFor(x => x.Options)
                .NotEmpty()
                .WithMessage("Phải có ít nhất 1 đáp án.")
                .When(x => x.Type == QuestionType.MultipleChoice || x.Type == QuestionType.MultipleAnswers);

            RuleFor(x => x.Options)
                .Must(options => options.Count >= 2)
                .WithMessage("Câu hỏi trắc nghiệm phải có ít nhất 2 đáp án.")
                .When(x => x.Type == QuestionType.MultipleChoice || x.Type == QuestionType.MultipleAnswers);

            RuleFor(x => x.Options)
                .Must(options => options.Any(o => o.IsCorrect))
                .WithMessage("Phải có ít nhất 1 đáp án đúng.")
                .When(x => x.Options != null && x.Options.Any());

            RuleFor(x => x.Options)
                .Must(options => options.Count(o => o.IsCorrect) == 1)
                .WithMessage("Câu hỏi một đáp án chỉ được có đúng 1 đáp án đúng.")
                .When(x => x.Type == QuestionType.MultipleChoice && x.Options != null && x.Options.Any());

            RuleFor(x => x.Explanation)
                .MaximumLength(2000)
                .WithMessage("Giải thích không được vượt quá 2000 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.Explanation));

            RuleFor(x => x.MediaUrl)
                .MaximumLength(500)
                .WithMessage("URL media không được vượt quá 500 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.MediaUrl));

            RuleForEach(x => x.Options)
                .SetValidator(new AnswerOptionCreateDtoValidator());
        }
    }

    public class AnswerOptionCreateDtoValidator : AbstractValidator<AnswerOptionCreateDto>
    {
        public AnswerOptionCreateDtoValidator()
        {
            RuleFor(x => x.Text)
                .NotEmpty()
                .WithMessage("Nội dung đáp án không được để trống.")
                .MaximumLength(1000)
                .WithMessage("Nội dung đáp án không được vượt quá 1000 ký tự.");

            RuleFor(x => x.OrderIndex)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Thứ tự đáp án phải lớn hơn hoặc bằng 0.");

            RuleFor(x => x.Feedback)
                .MaximumLength(1000)
                .WithMessage("Phản hồi không được vượt quá 1000 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.Feedback));

            RuleFor(x => x.MediaUrl)
                .MaximumLength(500)
                .WithMessage("URL media không được vượt quá 500 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.MediaUrl));
        }
    }

    public class QuestionUpdateDtoValidator : AbstractValidator<QuestionUpdateDto>
    {
        public QuestionUpdateDtoValidator()
        {
            Include(new QuestionCreateDtoValidator());
        }
    }

    public class QuestionBulkCreateDtoValidator : AbstractValidator<QuestionBulkCreateDto>
    {
        public QuestionBulkCreateDtoValidator()
        {
            RuleFor(x => x.Questions)
                .NotEmpty()
                .WithMessage("Danh sách câu hỏi không được để trống.");

            RuleFor(x => x.Questions)
                .Must(questions => questions.Count <= 100)
                .WithMessage("Không được tạo quá 100 câu hỏi cùng lúc.");

            RuleForEach(x => x.Questions)
                .SetValidator(new QuestionCreateDtoValidator());
        }
    }
}
