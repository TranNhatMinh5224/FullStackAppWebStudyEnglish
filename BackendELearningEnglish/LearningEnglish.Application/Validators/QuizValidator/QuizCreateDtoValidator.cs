using FluentValidation;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Validators.QuizValidator
{
    public class QuizCreateDtoValidator : AbstractValidator<QuizCreateDto>
    {
        public QuizCreateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Tiêu đề Quiz không được để trống")
                .MaximumLength(200)
                .WithMessage("Tiêu đề Quiz không được vượt quá 200 ký tự");

            RuleFor(x => x.AssessmentId)
                .GreaterThan(0)
                .WithMessage("AssessmentId phải lớn hơn 0");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Mô tả không được vượt quá 1000 ký tự");

            RuleFor(x => x.Instructions)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrEmpty(x.Instructions))
                .WithMessage("Hướng dẫn không được vượt quá 2000 ký tự");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Loại Quiz không hợp lệ");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Trạng thái Quiz không hợp lệ");

            RuleFor(x => x.TotalQuestions)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Tổng số câu hỏi phải >= 0");

            RuleFor(x => x.PassingScore)
                .GreaterThan(0)
                .When(x => x.PassingScore.HasValue)
                .WithMessage("Điểm đạt phải lớn hơn 0");

            // Duration phải > 0 nếu được set
            RuleFor(x => x.Duration)
                .GreaterThan(0)
                .When(x => x.Duration.HasValue)
                .WithMessage("Thời gian làm bài phải lớn hơn 0 phút");

            // AvailableFrom phải trong tương lai (nếu set)
            RuleFor(x => x.AvailableFrom)
                .GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-5))
                .When(x => x.AvailableFrom.HasValue)
                .WithMessage("Thời gian mở Quiz phải trong tương lai hoặc hiện tại");

            // MaxAttempts phải > 0 nếu không unlimited
            RuleFor(x => x.MaxAttempts)
                .GreaterThan(0)
                .When(x => x.AllowUnlimitedAttempts == false && x.MaxAttempts.HasValue)
                .WithMessage("Số lần làm tối đa phải lớn hơn 0");

            // Nếu AllowUnlimitedAttempts = false thì phải có MaxAttempts
            RuleFor(x => x.MaxAttempts)
                .NotNull()
                .When(x => x.AllowUnlimitedAttempts == false)
                .WithMessage("Phải chỉ định số lần làm tối đa khi không cho phép làm không giới hạn");

            // Nếu AllowUnlimitedAttempts = true thì MaxAttempts phải null
            RuleFor(x => x.MaxAttempts)
                .Null()
                .When(x => x.AllowUnlimitedAttempts == true)
                .WithMessage("MaxAttempts phải null khi cho phép làm không giới hạn");
        }
    }
}
