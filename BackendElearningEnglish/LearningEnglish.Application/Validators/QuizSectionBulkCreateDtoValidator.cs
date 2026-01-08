using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators
{
    public class QuizSectionBulkCreateDtoValidator : AbstractValidator<QuizSectionBulkCreateDto>
    {
        public QuizSectionBulkCreateDtoValidator()
        {
            RuleFor(x => x.QuizId)
                .GreaterThan(0)
                .WithMessage("QuizId phải lớn hơn 0.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Tiêu đề section không được để trống.")
                .MaximumLength(200)
                .WithMessage("Tiêu đề section không được vượt quá 200 ký tự.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được vượt quá 1000 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.QuizGroups)
                .NotNull()
                .WithMessage("Danh sách QuizGroups không được null.");

            // Validate ít nhất phải có groups HOẶC standalone questions
            RuleFor(x => x)
                .Must(dto => 
                    (dto.QuizGroups != null && dto.QuizGroups.Count > 0) ||
                    (dto.StandaloneQuestions != null && dto.StandaloneQuestions.Count > 0))
                .WithMessage("Section phải có ít nhất 1 QuizGroup hoặc 1 câu hỏi standalone.");

            RuleFor(x => x.QuizGroups)
                .Must(groups => groups == null || groups.Count <= 20)
                .WithMessage("Không được vượt quá 20 QuizGroups trong 1 section.");

            // Validate tổng số questions không vượt quá 100 (bao gồm cả trong groups và standalone)
            RuleFor(x => x)
                .Must(dto =>
                {
                    var questionsInGroups = dto.QuizGroups?.Sum(g => g.Questions?.Count ?? 0) ?? 0;
                    var standaloneCount = dto.StandaloneQuestions?.Count ?? 0;
                    var totalQuestions = questionsInGroups + standaloneCount;
                    return totalQuestions <= 100;
                })
                .WithMessage("Tổng số câu hỏi (trong groups + standalone) không được vượt quá 100.");

            RuleFor(x => x.StandaloneQuestions)
                .Must(questions => questions == null || questions.Count <= 50)
                .WithMessage("Không được vượt quá 50 câu hỏi standalone.");

            // Validate từng QuizGroup
            RuleForEach(x => x.QuizGroups)
                .SetValidator(new QuizGroupBulkCreateDtoValidator());

            // Validate từng standalone question
            RuleForEach(x => x.StandaloneQuestions)
                .SetValidator(new QuestionCreateDtoValidator());
        }
    }

    public class QuizGroupBulkCreateDtoValidator : AbstractValidator<QuizGroupBulkCreateDto>
    {
        public QuizGroupBulkCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên QuizGroup không được để trống.")
                .MaximumLength(200)
                .WithMessage("Tên QuizGroup không được vượt quá 200 ký tự.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Tiêu đề QuizGroup không được để trống.")
                .MaximumLength(200)
                .WithMessage("Tiêu đề QuizGroup không được vượt quá 200 ký tự.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được vượt quá 1000 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.SumScore)
                .GreaterThan(0)
                .WithMessage("SumScore phải lớn hơn 0.");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("DisplayOrder phải >= 0.");

            // Media validation
            RuleFor(x => x.ImgType)
                .MaximumLength(50)
                .WithMessage("ImgType không được vượt quá 50 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.ImgType));

            RuleFor(x => x.VideoType)
                .MaximumLength(50)
                .WithMessage("VideoType không được vượt quá 50 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.VideoType));

            RuleFor(x => x.VideoDuration)
                .GreaterThan(0)
                .WithMessage("VideoDuration phải lớn hơn 0.")
                .When(x => x.VideoDuration.HasValue);

            RuleFor(x => x.Questions)
                .NotNull()
                .WithMessage("Danh sách Questions không được null.");

            RuleFor(x => x.Questions)
                .Must(questions => questions != null && questions.Count > 0)
                .WithMessage("Mỗi QuizGroup phải có ít nhất 1 câu hỏi.");

            RuleFor(x => x.Questions)
                .Must(questions => questions != null && questions.Count <= 50)
                .WithMessage("Mỗi QuizGroup không được vượt quá 50 câu hỏi.");

            // Validate từng Question - sử dụng validator hiện có
            RuleForEach(x => x.Questions)
                .SetValidator(new QuestionCreateDtoValidator());
        }
    }
}
