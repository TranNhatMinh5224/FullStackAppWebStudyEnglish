using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.AssessmentValidators
{
    public class CreateAssessmentDtoValidator : AbstractValidator<CreateAssessmentDto>
    {
        public CreateAssessmentDtoValidator()
        {
            RuleFor(x => x.ModuleId)
                .GreaterThan(0)
                .WithMessage("Module ID phải lớn hơn 0");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Tiêu đề Assessment không được để trống")
                .Length(1, 200)
                .WithMessage("Tiêu đề Assessment phải có độ dài từ 1 đến 200 ký tự");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả Assessment không được vượt quá 1000 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Description));



            RuleFor(x => x.TotalPoints)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Tổng điểm phải lớn hơn hoặc bằng 0");

            RuleFor(x => x.PassingScore)
                .InclusiveBetween(0, 100)
                .WithMessage("Điểm đạt phải nằm trong khoảng từ 0 đến 100");

            RuleFor(x => x.OpenAt)
                .LessThan(x => x.DueAt)
                .When(x => x.OpenAt.HasValue && x.DueAt.HasValue)
                .WithMessage("Thời gian mở phải trước thời gian đóng");

            RuleFor(x => x.TimeLimit)
                .GreaterThan(TimeSpan.Zero)
                .When(x => x.TimeLimit.HasValue)
                .WithMessage("Thời gian giới hạn phải lớn hơn 0");
        }
    }
}
