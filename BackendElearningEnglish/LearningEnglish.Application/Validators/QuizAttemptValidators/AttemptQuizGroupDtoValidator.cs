using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.QuizAttemptValidators;

public class GroupItemDtoValidator : AbstractValidator<QuizItemDto>
{
    public GroupItemDtoValidator()
    {
        RuleFor(x => x.GroupId)
            .NotNull()
            .GreaterThan(0)
            .When(x => x.ItemType == "Group")
            .WithMessage("GroupId phải lớn hơn 0 khi ItemType là Group");

        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .When(x => x.ItemType == "Group")
            .WithMessage("Name không được null hoặc rỗng khi ItemType là Group");

        RuleFor(x => x.Questions)
            .NotNull()
            .NotEmpty()
            .When(x => x.ItemType == "Group")
            .WithMessage("Questions không được null hoặc rỗng khi ItemType là Group");
    }
}