using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Module;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class QuizAttemptAdminService : IQuizAttemptAdminService
    {
        private readonly IQuizAttemptService _quizAttemptService;
        private readonly IQuizAttemptRepository _quizAttemptRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IModuleProgressService _moduleProgressService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizAttemptAdminService> _logger;

        public QuizAttemptAdminService(
            IQuizAttemptService quizAttemptService,
            IQuizAttemptRepository quizAttemptRepository,
            IQuizRepository quizRepository,
            IAssessmentRepository assessmentRepository,
            IModuleProgressService moduleProgressService,
            INotificationRepository notificationRepository,
            IMapper mapper,
            ILogger<QuizAttemptAdminService> logger)
        {
            _quizAttemptService = quizAttemptService;
            _quizAttemptRepository = quizAttemptRepository;
            _quizRepository = quizRepository;
            _assessmentRepository = assessmentRepository;
            _moduleProgressService = moduleProgressService;
            _notificationRepository = notificationRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResponse<List<QuizAttemptDto>>> GetQuizAttemptsAsync(int quizId)
        {
            var response = new ServiceResponse<List<QuizAttemptDto>>();

            try
            {
                var attempts = await _quizAttemptRepository.GetByQuizIdAsync(quizId);
                var attemptDtos = _mapper.Map<List<QuizAttemptDto>>(attempts);
                response.Success = true;
                response.Data = attemptDtos;
                response.Message = $"Found {attempts.Count} attempts for quiz {quizId}";
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
            }

            return response;
        }

        // Lấy danh sách attempts với phân trang
        public async Task<ServiceResponse<PagedResult<QuizAttemptDto>>> GetQuizAttemptsPagedAsync(int quizId, PageRequest request)
        {
            var response = new ServiceResponse<PagedResult<QuizAttemptDto>>();

            try
            {
                var quizParams = new QuizAttemptQueryParameters
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
                var pagedResult = await _quizAttemptRepository.GetQuizAttemptsPagedAsync(quizId, quizParams);

                // Map entities to DTOs
                var attemptDtos = _mapper.Map<List<QuizAttemptDto>>(pagedResult.Items);
                var pagedDto = new PagedResult<QuizAttemptDto>
                {
                    Items = attemptDtos,
                    TotalCount = pagedResult.TotalCount,
                    PageNumber = pagedResult.PageNumber,
                    PageSize = pagedResult.PageSize
                };

                response.Success = true;
                response.Data = pagedDto;
                response.Message = $"Found {pagedResult.TotalCount} attempts for quiz {quizId}";
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
            }

            return response;
        }

        public async Task<ServiceResponse<QuizAttemptDetailDto>> GetAttemptDetailForReviewAsync(int attemptId)
        {
            var response = new ServiceResponse<QuizAttemptDetailDto>();

            try
            {
                // 1. Load attempt với User và Quiz
                var attempt = await _quizAttemptRepository.GetByIdAsync(attemptId);
                if (attempt == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy bài làm";
                    response.StatusCode = 404;
                    return response;
                }

                // 2. Load full quiz với questions và options
                var quiz = await _quizRepository.GetFullQuizAsync(attempt.QuizId);
                if (quiz == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy quiz";
                    response.StatusCode = 404;
                    return response;
                }

                // 3. Parse AnswersJson và ScoresJson
                var userAnswers = AnswerNormalizer.DeserializeAnswersJson(attempt.AnswersJson);
                var scores = AnswerNormalizer.DeserializeScoresJson(attempt.ScoresJson);

                // 4. Build QuizAttemptDetailDto
                var detailDto = new QuizAttemptDetailDto
                {
                    AttemptId = attempt.AttemptId,
                    QuizId = attempt.QuizId,
                    QuizTitle = quiz.Title,
                    UserId = attempt.UserId,
                    Email = attempt.User?.Email,
                    FirstName = attempt.User?.FirstName,
                    LastName = attempt.User?.LastName,
                    AttemptNumber = attempt.AttemptNumber,
                    StartedAt = attempt.StartedAt,
                    SubmittedAt = attempt.SubmittedAt,
                    Status = attempt.Status,
                    TimeSpentSeconds = attempt.TimeSpentSeconds,
                    TotalScore = attempt.TotalScore,
                    MaxScore = quiz.TotalQuestions > 0 ? quiz.TotalQuestions : 0,
                    Percentage = CalculatePercentageScore(attempt.TotalScore, quiz.TotalQuestions),
                    IsPassed = quiz.PassingScore.HasValue ? attempt.TotalScore >= quiz.PassingScore.Value : false,
                    Questions = new List<QuestionReviewDto>()
                };

                // 5. Collect all questions từ sections
                var allQuestions = new List<Question>();
                foreach (var section in quiz.QuizSections)
                {
                    foreach (var group in section.QuizGroups)
                    {
                        allQuestions.AddRange(group.Questions);
                    }
                    allQuestions.AddRange(section.Questions.Where(q => q.QuizGroupId == null));
                }

                // 6. Build QuestionReviewDto cho từng câu
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

                    detailDto.Questions.Add(questionReview);
                }

                response.Success = true;
                response.Data = detailDto;
                response.Message = "Lấy chi tiết bài làm thành công";
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
                _logger.LogError(ex, "Error getting attempt detail for review. AttemptId: {AttemptId}", attemptId);
            }

            return response;
        }

        private static object? ParseCorrectAnswer(Question question)
        {
            if (string.IsNullOrEmpty(question.CorrectAnswersJson))
                return null;

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<object>(question.CorrectAnswersJson);
            }
            catch
            {
                return question.CorrectAnswersJson;
            }
        }

        private static List<int> ParseUserAnswerAsOptionIds(object? userAnswer)
        {
            if (userAnswer == null)
                return new List<int>();

            try
            {
                if (userAnswer is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    return jsonElement.EnumerateArray()
                        .Where(e => e.ValueKind == System.Text.Json.JsonValueKind.Number)
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

        private static string BuildAnswerText(Question question, object? answer)
        {
            if (answer == null)
                return "Chưa trả lời";

            try
            {
                switch (question.Type)
                {
                    case Domain.Enums.QuestionType.MultipleChoice:
                    case Domain.Enums.QuestionType.TrueFalse:
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

                    case Domain.Enums.QuestionType.FillBlank:
                        return answer.ToString() ?? "N/A";

                    case Domain.Enums.QuestionType.Matching:
                        if (answer is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            var pairs = new List<string>();
                            foreach (var prop in jsonElement.EnumerateObject())
                            {
                                pairs.Add($"{prop.Name}→{prop.Value}");
                            }
                            return string.Join(", ", pairs);
                        }
                        return answer.ToString() ?? "N/A";

                    case Domain.Enums.QuestionType.Ordering:
                        if (answer is System.Text.Json.JsonElement jsonArr && jsonArr.ValueKind == System.Text.Json.JsonValueKind.Array)
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

        private static decimal CalculatePercentageScore(decimal totalScore, int totalQuestions)
        {
            if (totalQuestions <= 0)
                return 0;

            return Math.Round((totalScore / totalQuestions) * 100, 2);
        }

        public async Task<ServiceResponse<QuizAttemptResultDto>> ForceSubmitAttemptAsync(int attemptId)
        {
            var response = new ServiceResponse<QuizAttemptResultDto>();

            try
            {
                // 1. Load attempt (không cần check ownership - admin submit hộ)
                var attempt = await _quizAttemptRepository.GetByIdAsync(attemptId);
                if (attempt == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy bài làm";
                    response.StatusCode = 404;
                    return response;
                }

                // 2. Check: không submit lại bài đã nộp
                if (attempt.Status == QuizAttemptStatus.Submitted)
                {
                    response.Success = false;
                    response.Message = "Bài làm đã được nộp trước đó";
                    response.StatusCode = 400;
                    return response;
                }

                // 3. Load quiz để lấy settings
                var quiz = await _quizRepository.GetQuizByIdAsync(attempt.QuizId);
                if (quiz == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy quiz";
                    response.StatusCode = 404;
                    return response;
                }

                // 4. Update status → Submitted
                attempt.Status = QuizAttemptStatus.Submitted;
                attempt.SubmittedAt = DateTime.UtcNow;
                attempt.TimeSpentSeconds = (int)(attempt.SubmittedAt.Value - attempt.StartedAt).TotalSeconds;

                await _quizAttemptRepository.UpdateQuizAttemptAsync(attempt);

                // 5. Mark module as completed (nếu có)
                var assessment = await _assessmentRepository.GetAssessmentById(quiz.AssessmentId);
                if (assessment?.ModuleId != null)
                {
                    await _moduleProgressService.CompleteModuleAsync(attempt.UserId, assessment.ModuleId);
                }

                // 6. Tạo result DTO
                var result = new QuizAttemptResultDto
                {
                    AttemptId = attempt.AttemptId,
                    SubmittedAt = attempt.SubmittedAt.Value,
                    TimeSpentSeconds = attempt.TimeSpentSeconds
                };

                // 7. Tính điểm nếu teacher cho phép hiển thị ngay
                if (quiz.ShowScoreImmediately == true)
                {
                    result.TotalScore = attempt.TotalScore;

                    // Tính percentage
                    int totalQuestions = quiz.TotalQuestions;
                    int correctQuestions = 0;

                    if (!string.IsNullOrEmpty(attempt.ScoresJson))
                    {
                        var scores = AnswerNormalizer.DeserializeScoresJson(attempt.ScoresJson);
                        correctQuestions = scores.Count(s => s.Value > 0);
                        result.ScoresByQuestion = scores;
                    }

                    result.Percentage = totalQuestions > 0 
                        ? (decimal)((double)correctQuestions / totalQuestions) * 100 
                        : 0;

                    result.IsPassed = quiz.PassingScore.HasValue 
                        ? attempt.TotalScore >= quiz.PassingScore.Value 
                        : false;
                }

                // 8. Lấy đáp án đúng nếu teacher cho phép
                if (quiz.ShowAnswersAfterSubmit == true)
                {
                    result.Questions = QuizReviewBuilder.BuildQuestionReviewList(quiz, attempt);
                }

                // 9. Tạo notification cho học sinh
                try
                {
                    var notification = new Notification
                    {
                        UserId = attempt.UserId,
                        Title = "⏰ Bài quiz đã được nộp",
                        Message = $"Giáo viên đã nộp bài quiz '{quiz.Title}' hộ bạn." + 
                                  (quiz.ShowScoreImmediately == true 
                                      ? $" Điểm: {attempt.TotalScore}/{quiz.TotalQuestions}" 
                                      : " Kết quả sẽ được công bố sau."),
                        Type = NotificationType.AssessmentGraded,
                        RelatedEntityType = "Quiz",
                        RelatedEntityId = quiz.QuizId,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationRepository.AddAsync(notification);
                }
                catch (Exception notifEx)
                {
                    _logger.LogWarning(notifEx, "Failed to create notification for force-submitted attempt {AttemptId}", attemptId);
                }

                response.Success = true;
                response.Data = result;
                response.Message = "Đã nộp bài hộ học sinh thành công";
                response.StatusCode = 200;

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
                _logger.LogError(ex, "Error force submitting attempt {AttemptId}", attemptId);
                return response;
            }
        }

        public async Task<ServiceResponse<object>> GetQuizAttemptStatsAsync(int quizId)
        {
            var response = new ServiceResponse<object>();

            try
            {
                var attempts = await _quizAttemptRepository.GetByQuizIdAsync(quizId);
                var submittedAttempts = await _quizAttemptRepository.GetSubmittedAttemptsByQuizIdAsync(quizId);

                var stats = new
                {
                    TotalAttempts = attempts.Count,
                    CompletedAttempts = submittedAttempts.Count,
                    InProgressAttempts = attempts.Count - submittedAttempts.Count,
                    AverageScore = submittedAttempts.Any() ? submittedAttempts.Average(a => a.TotalScore) : 0,
                    HighestScore = submittedAttempts.Any() ? submittedAttempts.Max(a => a.TotalScore) : 0,
                    LowestScore = submittedAttempts.Any() ? submittedAttempts.Min(a => a.TotalScore) : 0
                };

                response.Success = true;
                response.Data = stats;
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
            }

            return response;
        }

        public async Task<ServiceResponse<List<QuizScoreDto>>> GetQuizScoresAsync(int quizId)
        {
            var response = new ServiceResponse<List<QuizScoreDto>>();

            try
            {
                var attempts = await _quizAttemptRepository.GetQuizScoresAsync(quizId);

                var mappedItems = _mapper.Map<List<QuizScoreDto>>(attempts);
                
                // Calculate Percentage and IsPassed for each item
                foreach (var item in mappedItems)
                {
                    var attempt = attempts.First(a => a.AttemptId == item.AttemptId);
                    item.Percentage = (decimal)CalculatePercentage(attempt, attempt.Quiz);
                    item.IsPassed = attempt.Quiz?.PassingScore.HasValue == true ? attempt.TotalScore >= attempt.Quiz.PassingScore.Value : false;
                }

                response.Success = true;
                response.Data = mappedItems;
                response.Message = $"Found {mappedItems.Count} completed attempts for quiz {quizId}";
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
            }

            return response;
        }

        // Lấy điểm của học sinh với phân trang
        public async Task<ServiceResponse<PagedResult<QuizScoreDto>>> GetQuizScoresPagedAsync(int quizId, PageRequest request)
        {
            var response = new ServiceResponse<PagedResult<QuizScoreDto>>();

            try
            {
                var quizParams = new QuizAttemptQueryParameters
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
                var pagedResult = await _quizAttemptRepository.GetQuizScoresPagedAsync(quizId, quizParams);

                var mappedItems = _mapper.Map<List<QuizScoreDto>>(pagedResult.Items);
                
                // Calculate Percentage and IsPassed for each item
                foreach (var item in mappedItems)
                {
                    var attempt = pagedResult.Items.First(a => a.AttemptId == item.AttemptId);
                    item.Percentage = (decimal)CalculatePercentage(attempt, attempt.Quiz);
                    item.IsPassed = attempt.Quiz?.PassingScore.HasValue == true ? attempt.TotalScore >= attempt.Quiz.PassingScore.Value : false;
                }

                var result = new PagedResult<QuizScoreDto>
                {
                    Items = mappedItems,
                    TotalCount = pagedResult.TotalCount,
                    PageNumber = pagedResult.PageNumber,
                    PageSize = pagedResult.PageSize
                };

                response.Success = true;
                response.Data = result;
                response.Message = $"Found {result.TotalCount} completed attempts for quiz {quizId}";
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
            }

            return response;
        }

        public async Task<ServiceResponse<List<QuizAttemptDto>>> GetUserQuizAttemptsAsync(int userId, int quizId)
        {
            var response = new ServiceResponse<List<QuizAttemptDto>>();

            try
            {
                var attempts = await _quizAttemptRepository.GetByUserAndQuizAsync(userId, quizId);
                var attemptDtos = _mapper.Map<List<QuizAttemptDto>>(attempts);
                response.Success = true;
                response.Data = attemptDtos;
                response.Message = $"Found {attempts.Count} attempts for user {userId} on quiz {quizId}";
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
            }

            return response;
        }

        private async Task<List<CorrectAnswerDto>> GetCorrectAnswersAsync(int quizId)
        {
            var correctAnswers = new List<CorrectAnswerDto>();

            var quizDetails = await _quizRepository.GetFullQuizAsync(quizId);
            if (quizDetails == null) return correctAnswers;

            foreach (var section in quizDetails.QuizSections)
            {
                foreach (var group in section.QuizGroups)
                {
                    foreach (var question in group.Questions)
                    {
                        correctAnswers.Add(CreateCorrectAnswerDto(question));
                    }
                }

                if (section.Questions != null)
                {
                    foreach (var question in section.Questions)
                    {
                        correctAnswers.Add(CreateCorrectAnswerDto(question));
                    }
                }
            }

            return correctAnswers;
        }

        private static CorrectAnswerDto CreateCorrectAnswerDto(Question question)
        {
            var correctOptions = new List<string>();

            if (question.Type == QuestionType.MultipleChoice || question.Type == QuestionType.TrueFalse)
            {
                var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
                if (correctOption != null)
                {
                    correctOptions.Add(correctOption.Text ?? string.Empty);
                }
            }
            else if (question.Type == QuestionType.MultipleAnswers)
            {
                correctOptions = question.Options
                    .Where(o => o.IsCorrect)
                    .Select(o => o.Text ?? string.Empty)
                    .ToList();
            }
            else if (question.Type == QuestionType.FillBlank || question.Type == QuestionType.Matching || question.Type == QuestionType.Ordering)
            {
                if (!string.IsNullOrEmpty(question.CorrectAnswersJson))
                {
                    try
                    {
                        correctOptions.Add($"Correct answer data: {question.CorrectAnswersJson}");
                    }
                    catch
                    {
                        correctOptions.Add("Correct answer available");
                    }
                }
            }

            return new CorrectAnswerDto
            {
                QuestionId = question.QuestionId,
                QuestionText = question.StemText,
                CorrectOptions = correctOptions
            };
        }

        private static double CalculatePercentage(QuizAttempt attempt, Quiz? quiz)
        {
            if (quiz == null || quiz.TotalQuestions <= 0) return 0;

            int correctQuestions = 0;

            if (!string.IsNullOrEmpty(attempt.ScoresJson))
            {
                var scores = AnswerNormalizer.DeserializeScoresJson(attempt.ScoresJson);
                correctQuestions = scores.Count(s => s.Value > 0);
            }

            return ((double)correctQuestions / quiz.TotalQuestions) * 100;
        }
    }
}
