using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.QuizAttemptValidators;

public class AttemptQuizGroupDtoValidator : AbstractValidator<AttemptQuizGroupDto>
{
    public AttemptQuizGroupDtoValidator()
    {
        RuleFor(x => x.GroupId)
            .GreaterThan(0)
            .WithMessage("GroupId phải lớn hơn 0");

        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage("Name không được null hoặc rỗng");

        RuleFor(x => x.Questions)
            .NotNull()
            .WithMessage("Questions không được null")
            .NotEmpty()
            .WithMessage("Questions không được rỗng");
    }
}
