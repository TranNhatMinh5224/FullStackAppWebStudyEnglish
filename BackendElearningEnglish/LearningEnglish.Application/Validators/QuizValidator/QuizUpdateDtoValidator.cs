using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.QuizValidator
{
    /// <summary>
    /// Validator cho QuizUpdateDto - kế thừa từ QuizCreateDtoValidator
    /// </summary>
    public class QuizUpdateDtoValidator : QuizCreateDtoValidator
    {
        public QuizUpdateDtoValidator() : base()
        {
            // Có thể thêm validation rules riêng cho Update nếu cần
            // Ví dụ: Không cho phép thay đổi AssessmentId sau khi tạo
            // RuleFor(x => x.AssessmentId)
            //     .Empty()
            //     .WithMessage("Không được thay đổi AssessmentId sau khi tạo Quiz");
        }
    }
}
