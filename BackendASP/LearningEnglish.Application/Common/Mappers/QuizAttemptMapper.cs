using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Common.Mappers
{
    /// <summary>
    /// Helper class để map Quiz entities sang DTOs cho quiz attempts
    /// </summary>
    public static class QuizAttemptMapper
    {
        private const string QuestionBucket = "questions";

        /// <summary>
        /// Shuffle quiz sections/groups/questions cho attempt
        /// </summary>
        public static List<AttemptQuizSectionDto> ShuffleQuizForAttempt(Quiz quiz, int attemptId)
        {
            var sections = new List<AttemptQuizSectionDto>();

            foreach (var section in quiz.QuizSections)
            {
                var sectionDto = new AttemptQuizSectionDto
                {
                    SectionId = section.QuizSectionId,
                    Title = section.Title,
                    Items = new List<QuizItemDto>()
                };

                // 1. Map groups sang GroupItemDto
                var groupItems = section.QuizGroups.Select(g => new GroupItemDto
                {
                    ItemType = "Group",
                    GroupId = g.QuizGroupId,
                    Name = g.Name,
                    ItemIndex = g.DisplayOrder,
                    Questions = g.Questions
                        .Select(q => MapToQuestionDto(q, attemptId, quiz.ShuffleAnswers.GetValueOrDefault(false)))
                        .ToList()
                }).ToList();

                // 2. Map standalone questions sang QuestionItemDto
                var standaloneQuestionItems = section.Questions
                    .Where(q => q.QuizGroupId == null)
                    .Select(q => MapToQuestionItemDto(q, attemptId, quiz.ShuffleAnswers.GetValueOrDefault(false)))
                    .ToList();

                // 3. Merge groups + questions, assign ItemIndex
                var allItems = new List<QuizItemDto>();

                // Lấy tất cả DisplayOrder của groups để tránh conflict
                var groupDisplayOrders = section.QuizGroups.Select(g => g.DisplayOrder).ToHashSet();

                int nextAvailableIndex = 0;
                foreach (var questionItem in standaloneQuestionItems)
                {
                    // Tìm ItemIndex không trùng với group DisplayOrder
                    while (groupDisplayOrders.Contains(nextAvailableIndex))
                    {
                        nextAvailableIndex++;
                    }
                    questionItem.ItemIndex = nextAvailableIndex++;
                }

                allItems.AddRange(groupItems);
                allItems.AddRange(standaloneQuestionItems);

                // 5. Sort theo ItemIndex
                sectionDto.Items = allItems.OrderBy(i => i.ItemIndex).ToList();

                sections.Add(sectionDto);
            }

            return sections;
        }

        /// <summary>
        /// Map Question entity sang QuestionDto (cho group questions)
        /// </summary>
        public static QuestionDto MapToQuestionDto(Question q, int attemptId, bool shuffleAnswers)
        {
            return new QuestionDto
            {
                QuestionId = q.QuestionId,
                QuestionText = q.StemText,
                MediaUrl = !string.IsNullOrWhiteSpace(q.MediaKey)
                    ? BuildPublicUrl.BuildURL(QuestionBucket, q.MediaKey)
                    : null,
                Type = q.Type,
                Points = q.Points,
                IsAnswered = false,
                CurrentScore = null,
                Options = MapToOptionDtos(q, attemptId, shuffleAnswers)
            };
        }

        /// <summary>
        /// Map Question entity sang QuestionItemDto (cho standalone questions)
        /// </summary>
        public static QuestionItemDto MapToQuestionItemDto(Question q, int attemptId, bool shuffleAnswers)
        {
            return new QuestionItemDto
            {
                ItemType = "Question",
                QuestionId = q.QuestionId,
                QuestionText = q.StemText,
                MediaUrl = !string.IsNullOrWhiteSpace(q.MediaKey)
                    ? BuildPublicUrl.BuildURL(QuestionBucket, q.MediaKey)
                    : null,
                Type = q.Type,
                Points = q.Points,
                IsAnswered = false,
                CurrentScore = null,
                Options = MapToOptionDtos(q, attemptId, shuffleAnswers)
            };
        }

        /// <summary>
        /// Map AnswerOptions sang AnswerOptionDto và shuffle nếu cần
        /// </summary>
        public static List<AnswerOptionDto> MapToOptionDtos(Question question, int attemptId, bool shuffleAnswers)
        {
            var options = question.Options.Select(o => new AnswerOptionDto
            {
                OptionId = o.AnswerOptionId,
                OptionText = o.Text ?? string.Empty,
                MediaUrl = !string.IsNullOrWhiteSpace(o.MediaKey)
                    ? BuildPublicUrl.BuildURL(QuestionBucket, o.MediaKey)
                    : null
            }).ToList();

            // Shuffle options nếu bật và question type phù hợp
            if (shuffleAnswers && QuizShuffleHelper.ShouldShuffleAnswers(question.Type))
            {
                QuizShuffleHelper.ShuffleAnswers(options, attemptId, question.QuestionId);
            }

            return options;
        }
    }
}
