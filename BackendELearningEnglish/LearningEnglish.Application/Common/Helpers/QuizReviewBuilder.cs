using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using System.Text.Json;

namespace LearningEnglish.Application.Common.Helpers
{

    public static class QuizReviewBuilder
    {
        
        public static List<QuestionReviewDto> BuildQuestionReviewList(Quiz quiz, QuizAttempt attempt)
        {
            var questionReviews = new List<QuestionReviewDto>();

            // Parse user answers và scores
            var userAnswers = AnswerNormalizer.DeserializeAnswersJson(attempt.AnswersJson);
            var scores = AnswerNormalizer.DeserializeScoresJson(attempt.ScoresJson);

            // Collect all questions từ sections
            var allQuestions = new List<Question>();
            foreach (var section in quiz.QuizSections)
            {
                foreach (var group in section.QuizGroups)
                {
                    allQuestions.AddRange(group.Questions);
                }
                allQuestions.AddRange(section.Questions.Where(q => q.QuizGroupId == null));
            }

            // Build QuestionReviewDto cho từng câu
            foreach (var question in allQuestions.OrderBy(q => q.QuestionId))
            {
                var questionReview = new QuestionReviewDto
                {
                    QuestionId = question.QuestionId,
                    QuestionText = question.StemText,
                    MediaUrl = question.MediaKey,
                    Type = question.Type,
                    Points = question.Points,
                    Score = scores.ContainsKey(question.QuestionId) ? scores[question.QuestionId] : 0,
                    IsCorrect = scores.ContainsKey(question.QuestionId) && scores[question.QuestionId] >= question.Points,
                    UserAnswer = userAnswers.ContainsKey(question.QuestionId) ? userAnswers[question.QuestionId] : null,
                    CorrectAnswer = ParseCorrectAnswer(question),
                    Options = new List<AnswerOptionReviewDto>()
                };

                questionReview.UserAnswerText = BuildAnswerText(question, questionReview.UserAnswer);
                questionReview.CorrectAnswerText = BuildAnswerText(question, questionReview.CorrectAnswer);

                // Build options nếu có
                if (question.Options != null && question.Options.Any())
                {
                    var userAnswerIds = ParseUserAnswerAsOptionIds(questionReview.UserAnswer);

                    foreach (var option in question.Options.OrderBy(o => o.AnswerOptionId))
                    {
                        questionReview.Options.Add(new AnswerOptionReviewDto
                        {
                            OptionId = option.AnswerOptionId,
                            OptionText = option.Text ?? string.Empty,
                            MediaUrl = option.MediaKey,
                            IsCorrect = option.IsCorrect,
                            IsSelected = userAnswerIds.Contains(option.AnswerOptionId)
                        });
                    }
                }

                questionReviews.Add(questionReview);
            }

            return questionReviews;
        }

       
        public static object? ParseCorrectAnswer(Question question)
        {
            if (string.IsNullOrEmpty(question.CorrectAnswersJson))
                return null;

            try
            {
                return JsonSerializer.Deserialize<object>(question.CorrectAnswersJson);
            }
            catch
            {
                return question.CorrectAnswersJson;
            }
        }

       
        public static List<int> ParseUserAnswerAsOptionIds(object? userAnswer)
        {
            if (userAnswer == null)
                return new List<int>();

            try
            {
                if (userAnswer is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                {
                    return jsonElement.EnumerateArray()
                        .Where(e => e.ValueKind == JsonValueKind.Number)
                        .Select(e => e.GetInt32())
                        .ToList();
                }

                if (int.TryParse(userAnswer.ToString(), out int singleId))
                {
                    return new List<int> { singleId };
                }
            }
            catch { }

            return new List<int>();
        }

        public static string BuildAnswerText(Question question, object? answer)
        {
            if (answer == null)
                return "Chưa trả lời";

            try
            {
                switch (question.Type)
                {
                    case QuestionType.MultipleChoice:
                    case QuestionType.TrueFalse:
                        var optionIds = ParseUserAnswerAsOptionIds(answer);
                        if (optionIds.Any() && question.Options != null)
                        {
                            var selectedOptions = question.Options
                                .Where(o => optionIds.Contains(o.AnswerOptionId))
                                .Select(o => o.Text ?? "N/A")
                                .ToList();
                            return string.Join(", ", selectedOptions);
                        }
                        return answer.ToString() ?? "N/A";

                    case QuestionType.FillBlank:
                        return answer.ToString() ?? "N/A";

                    case QuestionType.Matching:
                        if (answer is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                        {
                            var pairs = new List<string>();
                            foreach (var prop in jsonElement.EnumerateObject())
                            {
                                pairs.Add($"{prop.Name}→{prop.Value}");
                            }
                            return string.Join(", ", pairs);
                        }
                        return answer.ToString() ?? "N/A";

                    case QuestionType.Ordering:
                        if (answer is JsonElement jsonArr && jsonArr.ValueKind == JsonValueKind.Array)
                        {
                            var items = jsonArr.EnumerateArray().Select(e => e.ToString()).ToList();
                            return string.Join(", ", items);
                        }
                        return answer.ToString() ?? "N/A";

                    default:
                        return answer.ToString() ?? "N/A";
                }
            }
            catch
            {
                return answer.ToString() ?? "N/A";
            }
        }
    }
}
