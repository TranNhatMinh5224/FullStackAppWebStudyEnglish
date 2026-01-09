using FluentValidation;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Validators.QuizAttemptValidators
{
    // Validator cho SingleChoiceAnswerDto (MultipleChoice, TrueFalse)
    public class SingleChoiceAnswerDtoValidator : AbstractValidator<SingleChoiceAnswerDto>
    {
        public SingleChoiceAnswerDtoValidator()
        {
            RuleFor(x => x.QuestionId)
                .GreaterThan(0)
                .WithMessage("QuestionId phải lớn hơn 0");

            RuleFor(x => x.SelectedOptionId)
                .GreaterThan(0)
                .WithMessage("SelectedOptionId phải lớn hơn 0");
        }
    }

    // Validator cho MultipleChoiceAnswerDto (MultipleAnswers)
    public class MultipleChoiceAnswerDtoValidator : AbstractValidator<MultipleChoiceAnswerDto>
    {
        public MultipleChoiceAnswerDtoValidator()
        {
            RuleFor(x => x.QuestionId)
                .GreaterThan(0)
                .WithMessage("QuestionId phải lớn hơn 0");

            RuleFor(x => x.SelectedOptionIds)
                .NotEmpty()
                .WithMessage("Phải chọn ít nhất 1 đáp án")
                .Must(ids => ids.All(id => id > 0))
                .WithMessage("Tất cả SelectedOptionIds phải lớn hơn 0")
                .Must(ids => ids.Count == ids.Distinct().Count())
                .WithMessage("Không được chọn trùng đáp án");
        }
    }

    // Validator cho FillBlankAnswerDto
    public class FillBlankAnswerDtoValidator : AbstractValidator<FillBlankAnswerDto>
    {
        public FillBlankAnswerDtoValidator()
        {
            RuleFor(x => x.QuestionId)
                .GreaterThan(0)
                .WithMessage("QuestionId phải lớn hơn 0");

            RuleFor(x => x.AnswerText)
                .NotEmpty()
                .WithMessage("AnswerText không được để trống")
                .MaximumLength(1000)
                .WithMessage("AnswerText không được vượt quá 1000 ký tự");
        }
    }

    // Validator cho MatchingAnswerDto
    public class MatchingAnswerDtoValidator : AbstractValidator<MatchingAnswerDto>
    {
        public MatchingAnswerDtoValidator()
        {
            RuleFor(x => x.QuestionId)
                .GreaterThan(0)
                .WithMessage("QuestionId phải lớn hơn 0");

            RuleFor(x => x.Matches)
                .NotEmpty()
                .WithMessage("Phải có ít nhất 1 cặp ghép nối")
                .Must(matches => matches.Keys.All(k => k > 0) && matches.Values.All(v => v > 0))
                .WithMessage("Tất cả keys và values trong Matches phải lớn hơn 0");
        }
    }

    // Validator cho OrderingAnswerDto
    public class OrderingAnswerDtoValidator : AbstractValidator<OrderingAnswerDto>
    {
        public OrderingAnswerDtoValidator()
        {
            RuleFor(x => x.QuestionId)
                .GreaterThan(0)
                .WithMessage("QuestionId phải lớn hơn 0");

            RuleFor(x => x.OrderedOptionIds)
                .NotEmpty()
                .WithMessage("Phải có ít nhất 1 option trong thứ tự")
                .Must(ids => ids.All(id => id > 0))
                .WithMessage("Tất cả OrderedOptionIds phải lớn hơn 0")
                .Must(ids => ids.Count == ids.Distinct().Count())
                .WithMessage("Không được có option trùng lặp trong thứ tự");
        }
    }

    // NOTE: UpdateAnswerRequestDtoValidator đã được định nghĩa trong file UpdateAnswerRequestDtoValidator.cs
    // Không cần định nghĩa lại ở đây để tránh conflict
}

