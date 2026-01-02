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
    public class QuizAttemptTeacherService : IQuizAttemptTeacherService
    {
        private readonly IQuizAttemptRepository _quizAttemptRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IModuleProgressService _moduleProgressService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizAttemptTeacherService> _logger;

        public QuizAttemptTeacherService(
            IQuizAttemptRepository quizAttemptRepository,
            IQuizRepository quizRepository,
            IAssessmentRepository assessmentRepository,
            IModuleRepository moduleRepository,
            ICourseRepository courseRepository,
            IModuleProgressService moduleProgressService,
            INotificationRepository notificationRepository,
            IMapper mapper,
            ILogger<QuizAttemptTeacherService> logger)
        {
            _quizAttemptRepository = quizAttemptRepository;
            _quizRepository = quizRepository;
            _assessmentRepository = assessmentRepository;
            _moduleRepository = moduleRepository;
            _courseRepository = courseRepository;
            _moduleProgressService = moduleProgressService;
            _notificationRepository = notificationRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // Helper method: Check if teacher owns or is enrolled in the course containing the quiz
        private async Task<ServiceResponse<bool>> CheckTeacherAccessToQuizAsync(int quizId, int teacherId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
                if (quiz == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy Quiz";
                    response.StatusCode = 404;
                    response.Data = false;
                    return response;
                }

                // Lấy Assessment với Module và Course để check
                var assessment = await _assessmentRepository.GetAssessmentById(quiz.AssessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy Assessment";
                    response.StatusCode = 404;
                    response.Data = false;
                    return response;
                }

                // Lấy Module với Course để check
                var module = await _moduleRepository.GetModuleWithCourseAsync(assessment.ModuleId);
                if (module == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy Module";
                    response.StatusCode = 404;
                    response.Data = false;
                    return response;
                }

                // Check: teacher phải là owner HOẶC đã enroll
                var courseId = module.Lesson?.CourseId;
                if (!courseId.HasValue)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy khóa học";
                    response.StatusCode = 404;
                    response.Data = false;
                    return response;
                }

                var course = await _courseRepository.GetCourseById(courseId.Value);
                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy khóa học";
                    response.StatusCode = 404;
                    response.Data = false;
                    return response;
                }

                var isOwner = course.TeacherId.HasValue && course.TeacherId.Value == teacherId;
                var isEnrolled = await _courseRepository.IsUserEnrolled(courseId.Value, teacherId);

                if (!isOwner && !isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn cần sở hữu hoặc đăng ký khóa học để xem attempts của Quiz này";
                    response.Data = false;
                    _logger.LogWarning("Teacher {TeacherId} attempted to access quiz attempts {QuizId} without ownership or enrollment", 
                        teacherId, quizId);
                    return response;
                }

                response.Success = true;
                response.Data = true;
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking teacher access to quiz {QuizId} for teacher {TeacherId}", quizId, teacherId);
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
                response.Data = false;
            }

            return response;
        }

        public async Task<ServiceResponse<List<QuizAttemptDto>>> GetQuizAttemptsAsync(int quizId, int teacherId)
        {
            var response = new ServiceResponse<List<QuizAttemptDto>>();

            // Check ownership/enrollment
            var accessCheck = await CheckTeacherAccessToQuizAsync(quizId, teacherId);
            if (!accessCheck.Success)
            {
                return new ServiceResponse<List<QuizAttemptDto>>
                {
                    Success = false,
                    StatusCode = accessCheck.StatusCode,
                    Message = accessCheck.Message
                };
            }

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
                _logger.LogError(ex, "Error getting quiz attempts for quiz {QuizId} and teacher {TeacherId}", quizId, teacherId);
            }

            return response;
        }

        public async Task<ServiceResponse<PagedResult<QuizAttemptDto>>> GetQuizAttemptsPagedAsync(int quizId, int teacherId, PageRequest request)
        {
            var response = new ServiceResponse<PagedResult<QuizAttemptDto>>();

            // Check ownership/enrollment
            var accessCheck = await CheckTeacherAccessToQuizAsync(quizId, teacherId);
            if (!accessCheck.Success)
            {
                return new ServiceResponse<PagedResult<QuizAttemptDto>>
                {
                    Success = false,
                    StatusCode = accessCheck.StatusCode,
                    Message = accessCheck.Message
                };
            }

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
                _logger.LogError(ex, "Error getting paged quiz attempts for quiz {QuizId} and teacher {TeacherId}", quizId, teacherId);
            }

            return response;
        }

        public async Task<ServiceResponse<object>> GetQuizAttemptStatsAsync(int quizId, int teacherId)
        {
            var response = new ServiceResponse<object>();

            // Check ownership/enrollment
            var accessCheck = await CheckTeacherAccessToQuizAsync(quizId, teacherId);
            if (!accessCheck.Success)
            {
                return new ServiceResponse<object>
                {
                    Success = false,
                    StatusCode = accessCheck.StatusCode,
                    Message = accessCheck.Message
                };
            }

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
                _logger.LogError(ex, "Error getting quiz attempt stats for quiz {QuizId} and teacher {TeacherId}", quizId, teacherId);
            }

            return response;
        }

        public async Task<ServiceResponse<List<QuizScoreDto>>> GetQuizScoresAsync(int quizId, int teacherId)
        {
            var response = new ServiceResponse<List<QuizScoreDto>>();

            // Check ownership/enrollment
            var accessCheck = await CheckTeacherAccessToQuizAsync(quizId, teacherId);
            if (!accessCheck.Success)
            {
                return new ServiceResponse<List<QuizScoreDto>>
                {
                    Success = false,
                    StatusCode = accessCheck.StatusCode,
                    Message = accessCheck.Message
                };
            }

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
                _logger.LogError(ex, "Error getting quiz scores for quiz {QuizId} and teacher {TeacherId}", quizId, teacherId);
            }

            return response;
        }

        public async Task<ServiceResponse<PagedResult<QuizScoreDto>>> GetQuizScoresPagedAsync(int quizId, int teacherId, PageRequest request)
        {
            var response = new ServiceResponse<PagedResult<QuizScoreDto>>();

            // Check ownership/enrollment
            var accessCheck = await CheckTeacherAccessToQuizAsync(quizId, teacherId);
            if (!accessCheck.Success)
            {
                return new ServiceResponse<PagedResult<QuizScoreDto>>
                {
                    Success = false,
                    StatusCode = accessCheck.StatusCode,
                    Message = accessCheck.Message
                };
            }

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
                _logger.LogError(ex, "Error getting paged quiz scores for quiz {QuizId} and teacher {TeacherId}", quizId, teacherId);
            }

            return response;
        }

        public async Task<ServiceResponse<List<QuizAttemptDto>>> GetUserQuizAttemptsAsync(int userId, int quizId, int teacherId)
        {
            var response = new ServiceResponse<List<QuizAttemptDto>>();

            // Check ownership/enrollment
            var accessCheck = await CheckTeacherAccessToQuizAsync(quizId, teacherId);
            if (!accessCheck.Success)
            {
                return new ServiceResponse<List<QuizAttemptDto>>
                {
                    Success = false,
                    StatusCode = accessCheck.StatusCode,
                    Message = accessCheck.Message
                };
            }

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
                _logger.LogError(ex, "Error getting user quiz attempts for user {UserId}, quiz {QuizId} and teacher {TeacherId}", userId, quizId, teacherId);
            }

            return response;
        }

        public async Task<ServiceResponse<QuizAttemptDetailDto>> GetAttemptDetailForReviewAsync(int attemptId, int teacherId)
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

                // 2. Check teacher access
                var accessCheck = await CheckTeacherAccessToQuizAsync(attempt.QuizId, teacherId);
                if (!accessCheck.Success)
                {
                    response.Success = false;
                    response.StatusCode = accessCheck.StatusCode;
                    response.Message = accessCheck.Message;
                    return response;
                }

                // 3. Load full quiz với questions và options
                var quiz = await _quizRepository.GetFullQuizAsync(attempt.QuizId);
                if (quiz == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy quiz";
                    response.StatusCode = 404;
                    return response;
                }

                // 4. Parse AnswersJson và ScoresJson
                var userAnswers = AnswerNormalizer.DeserializeAnswersJson(attempt.AnswersJson);
                var scores = AnswerNormalizer.DeserializeScoresJson(attempt.ScoresJson);

                // 5. Build QuizAttemptDetailDto
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

                // 6. Collect all questions từ sections
                var allQuestions = new List<Question>();
                foreach (var section in quiz.QuizSections)
                {
                    // Questions trong groups
                    foreach (var group in section.QuizGroups)
                    {
                        allQuestions.AddRange(group.Questions);
                    }
                    
                    // Standalone questions
                    allQuestions.AddRange(section.Questions.Where(q => q.QuizGroupId == null));
                }

                // 7. Build QuestionReviewDto cho từng câu
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

                    // Build human-readable answer text
                    questionReview.UserAnswerText = BuildAnswerText(question, questionReview.UserAnswer);
                    questionReview.CorrectAnswerText = BuildAnswerText(question, questionReview.CorrectAnswer);

                    // Build options với IsCorrect và IsSelected
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
                _logger.LogError(ex, "Error getting attempt detail for review. AttemptId: {AttemptId}, TeacherId: {TeacherId}", attemptId, teacherId);
            }

            return response;
        }

        // Helper: Parse CorrectAnswersJson to object
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

        // Helper: Parse user answer as list of option IDs
        private static List<int> ParseUserAnswerAsOptionIds(object? userAnswer)
        {
            if (userAnswer == null)
                return new List<int>();

            try
            {
                // MultipleChoice: [1, 2, 3]
                if (userAnswer is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    return jsonElement.EnumerateArray()
                        .Where(e => e.ValueKind == System.Text.Json.JsonValueKind.Number)
                        .Select(e => e.GetInt32())
                        .ToList();
                }

                // Single value: 1
                if (int.TryParse(userAnswer.ToString(), out int singleId))
                {
                    return new List<int> { singleId };
                }
            }
            catch { }

            return new List<int>();
        }

        // Helper: Build human-readable answer text
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
                        // Format: {"1": 2, "3": 4} -> "1→2, 3→4"
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
                        // Format: [2, 1, 3] -> "2, 1, 3"
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

        // Helper: Calculate percentage score
        private static decimal CalculatePercentageScore(decimal totalScore, int totalQuestions)
        {
            if (totalQuestions <= 0)
                return 0;

            return Math.Round((totalScore / totalQuestions) * 100, 2);
        }

        private static double CalculatePercentage(QuizAttempt attempt, Quiz? quiz)
        {
            if (quiz == null || quiz.TotalQuestions <= 0) return 0;

            if (!string.IsNullOrEmpty(attempt.ScoresJson))
            {
                var scores = AnswerNormalizer.DeserializeScoresJson(attempt.ScoresJson);
                int correctQuestions = scores.Count(s => s.Value > 0);
                return ((double)correctQuestions / quiz.TotalQuestions) * 100;
            }

            return 0;
        }

        // Force submit attempt (for when student violates rules, forgets to submit, or has technical issues)
        public async Task<ServiceResponse<QuizAttemptResultDto>> ForceSubmitAttemptAsync(int attemptId, int teacherId)
        {
            var response = new ServiceResponse<QuizAttemptResultDto>();

            try
            {
                // 1. Load attempt
                var attempt = await _quizAttemptRepository.GetByIdAsync(attemptId);
                if (attempt == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy bài làm";
                    response.StatusCode = 404;
                    return response;
                }

                // 2. Check teacher access to this quiz
                var accessCheck = await CheckTeacherAccessToQuizAsync(attempt.QuizId, teacherId);
                if (!accessCheck.Success || !accessCheck.Data)
                {
                    response.Success = false;
                    response.Message = accessCheck.Message ?? "Bạn không có quyền truy cập quiz này";
                    response.StatusCode = accessCheck.StatusCode;
                    return response;
                }

                // 3. Check: cannot re-submit already submitted attempt
                if (attempt.Status == QuizAttemptStatus.Submitted)
                {
                    response.Success = false;
                    response.Message = "Bài làm đã được nộp trước đó";
                    response.StatusCode = 400;
                    return response;
                }

                // 4. Load quiz to get settings
                var quiz = await _quizRepository.GetQuizByIdAsync(attempt.QuizId);
                if (quiz == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy quiz";
                    response.StatusCode = 404;
                    return response;
                }

                // 5. Update status → Submitted
                attempt.Status = QuizAttemptStatus.Submitted;
                attempt.SubmittedAt = DateTime.UtcNow;
                attempt.TimeSpentSeconds = (int)(attempt.SubmittedAt.Value - attempt.StartedAt).TotalSeconds;

                await _quizAttemptRepository.UpdateQuizAttemptAsync(attempt);

                // 6. Mark module as completed (if applicable)
                var assessment = await _assessmentRepository.GetAssessmentById(quiz.AssessmentId);
                if (assessment?.ModuleId != null)
                {
                    await _moduleProgressService.CompleteModuleAsync(attempt.UserId, assessment.ModuleId);
                }

                // 7. Create result DTO
                var result = new QuizAttemptResultDto
                {
                    AttemptId = attempt.AttemptId,
                    SubmittedAt = attempt.SubmittedAt.Value,
                    TimeSpentSeconds = attempt.TimeSpentSeconds
                };

                // 8. Calculate score if teacher allows showing immediately
                if (quiz.ShowScoreImmediately == true)
                {
                    result.TotalScore = attempt.TotalScore;

                    // Calculate percentage
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

                // 9. Get correct answers if teacher allows
                if (quiz.ShowAnswersAfterSubmit == true)
                {
                    result.CorrectAnswers = await GetCorrectAnswersAsync(attempt.QuizId);
                }

                // 10. Create notification for student
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

        // Helper method to get correct answers for a quiz
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

        // Helper method to create correct answer DTO based on question type
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
    }
}
