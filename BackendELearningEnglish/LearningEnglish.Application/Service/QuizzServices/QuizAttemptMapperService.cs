using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using LearningEnglish.Application.Interface.Services;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Service;

/// <summary>
/// Implementation của IQuizAttemptMapper
/// Tuân thủ Clean Architecture + SOLID:
/// - Dependency Inversion: Inject IQuestionMediaService, IQuizGroupMediaService
/// - Single Responsibility: Chỉ map Quiz entities sang DTOs
/// - KHÔNG có bucket names, BuildPublicUrl - delegate cho MediaServices
/// </summary>
public class QuizAttemptMapperService : IQuizAttemptMapper
{
    private readonly IQuestionMediaService _questionMediaService;
    private readonly IQuizGroupMediaService _quizGroupMediaService;

    public QuizAttemptMapperService(
        IQuestionMediaService questionMediaService,
        IQuizGroupMediaService quizGroupMediaService)
    {
        _questionMediaService = questionMediaService;
        _quizGroupMediaService = quizGroupMediaService;
    }

    /// <inheritdoc/>
    public List<AttemptQuizSectionDto> ShuffleQuizForAttempt(Quiz quiz, int attemptId)
    {
        var sections = new List<AttemptQuizSectionDto>();

        foreach (var section in quiz.QuizSections)
        {
            var sectionDto = new AttemptQuizSectionDto
            {
                SectionId = section.QuizSectionId,
                QuizId = section.QuizId,  // ID quiz
                Title = section.Title,
                Description = section.Description,  // Mô tả section
                DisplayOrder = 0,  // TODO: Cần thêm DisplayOrder vào QuizSection entity
                Items = new List<QuizItemDto>()
            };

            // 1. Map groups sang QuizItemDto
            var groupItems = section.QuizGroups.Select(g => new QuizItemDto
            {
                ItemType = "Group",
                ItemIndex = g.DisplayOrder,
                
                // Group properties - MAP TẤT CẢ FIELDS TỪ ENTITY
                GroupId = g.QuizGroupId,
                Name = g.Name,
                Title = g.Title,  // Tiêu đề group (Photographs, Short Conversations)
                Description = g.Description,  // Đoạn văn/bài đọc cho Reading Comprehension
                ImgUrl = _quizGroupMediaService.BuildImageUrl(g.ImgKey),
                ImgType = g.ImgType,  // image/jpeg, image/png
                VideoUrl = _quizGroupMediaService.BuildVideoUrl(g.VideoKey),
                VideoType = g.VideoType,  // video/mp4
                VideoDuration = g.VideoDuration,  // Độ dài video (seconds)
                SumScore = g.SumScore,  // Tổng điểm của group
                Questions = g.Questions
                    .Select(q => MapToQuestionDto(q, attemptId, quiz.ShuffleAnswers.GetValueOrDefault(false)))
                    .ToList()
            }).ToList();

            // 2. Map standalone questions sang QuizItemDto
            var standaloneQuestionItems = section.Questions
                .Where(q => q.QuizGroupId == null)
                .Select(q => MapToStandaloneQuestionItemDto(q, attemptId, quiz.ShuffleAnswers.GetValueOrDefault(false)))
                .ToList();

            // 3. Merge groups + questions vào cùng list
            var allItems = new List<QuizItemDto>();
            allItems.AddRange(groupItems);
            allItems.AddRange(standaloneQuestionItems);

            // 4. Sort theo ItemIndex để xen kẽ Groups và Questions
            sectionDto.Items = allItems.OrderBy(i => i.ItemIndex).ToList();

            sections.Add(sectionDto);
        }

        return sections;
    }

    /// <inheritdoc/>
    public QuestionDto MapToQuestionDto(Question q, int attemptId, bool shuffleAnswers)
    {
        return new QuestionDto
        {
            QuestionId = q.QuestionId,
            QuestionText = q.StemText,
            // Sử dụng MediaService
            MediaUrl = _questionMediaService.BuildMediaUrl(q.MediaKey),
            MediaType = q.MediaType,  // Loại media (image/png, audio/mpeg)
            Type = q.Type,
            Points = q.Points,
            DisplayOrder = q.DisplayOrder,  // Thứ tự hiển thị
            IsAnswered = false,
            CurrentScore = null,
            Options = MapToOptionDtos(q, attemptId, shuffleAnswers)
        };
    }

    /// <inheritdoc/>
    public QuizItemDto MapToStandaloneQuestionItemDto(Question q, int attemptId, bool shuffleAnswers)
    {
        return new QuizItemDto
        {
            ItemType = "Question",
            ItemIndex = q.DisplayOrder,
            
            // Question properties
            QuestionId = q.QuestionId,
            QuestionText = q.StemText,
            // Sử dụng MediaService
            MediaUrl = _questionMediaService.BuildMediaUrl(q.MediaKey),
            Type = q.Type,
            Points = q.Points,
            IsAnswered = false,
            CurrentScore = null,
            Options = MapToOptionDtos(q, attemptId, shuffleAnswers)
        };
    }

    /// <inheritdoc/>
    public List<AnswerOptionDto> MapToOptionDtos(Question question, int attemptId, bool shuffleAnswers)
    {
        var options = question.Options.Select(o => new AnswerOptionDto
        {
            OptionId = o.AnswerOptionId,
            OptionText = o.Text ?? string.Empty,
            // Sử dụng MediaService
            MediaUrl = _questionMediaService.BuildMediaUrl(o.MediaKey),
            MediaType = o.MediaType  // Loại media (image/png, audio/mpeg)
        }).ToList();

        // Shuffle options nếu bật và question type phù hợp
        if (shuffleAnswers && QuizShuffleHelper.ShouldShuffleAnswers(question.Type))
        {
            QuizShuffleHelper.ShuffleAnswers(options, attemptId, question.QuestionId);
        }

        return options;
    }
}
