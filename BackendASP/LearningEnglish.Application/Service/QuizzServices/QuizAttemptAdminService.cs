using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Common.Helpers;

namespace LearningEnglish.Application.Service
{
    public class QuizAttemptAdminService : IQuizAttemptAdminService
    {
        private readonly IQuizAttemptService _quizAttemptService;
        private readonly IQuizAttemptRepository _quizAttemptRepository;

        public QuizAttemptAdminService(
            IQuizAttemptService quizAttemptService,
            IQuizAttemptRepository quizAttemptRepository)
        {
            _quizAttemptService = quizAttemptService;
            _quizAttemptRepository = quizAttemptRepository;
        }

        public async Task<ServiceResponse<List<QuizAttempt>>> GetQuizAttemptsAsync(int quizId)
        {
            var response = new ServiceResponse<List<QuizAttempt>>();

            try
            {
                var attempts = await _quizAttemptRepository.GetByQuizIdAsync(quizId);
                response.Success = true;
                response.Data = attempts;
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
        public async Task<ServiceResponse<PagedResult<QuizAttempt>>> GetQuizAttemptsPagedAsync(int quizId, PageRequest request)
        {
            var response = new ServiceResponse<PagedResult<QuizAttempt>>();

            try
            {
                var pagedResult = await _quizAttemptRepository.GetQuizAttemptsPagedAsync(quizId, request);

                response.Success = true;
                response.Data = pagedResult;
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

        public async Task<ServiceResponse<QuizAttempt>> GetAttemptDetailsAsync(int attemptId)
        {
            var response = new ServiceResponse<QuizAttempt>();

            try
            {
                var attempt = await _quizAttemptRepository.GetByIdAsync(attemptId);
                if (attempt == null)
                {
                    response.Success = false;
                    response.Message = "Attempt not found";
                    response.StatusCode = 404;
                    return response;
                }

                response.Success = true;
                response.Data = attempt;
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

        public async Task<ServiceResponse<QuizAttemptResultDto>> ForceSubmitAttemptAsync(int attemptId)
        {
            return await _quizAttemptService.SubmitQuizAttemptAsync(attemptId);
        }

        public async Task<ServiceResponse<object>> GetQuizAttemptStatsAsync(int quizId)
        {
            var response = new ServiceResponse<object>();

            try
            {
                var attempts = await _quizAttemptRepository.GetByQuizIdAsync(quizId);
                var submittedAttempts = attempts.Where(a => a.Status == QuizAttemptStatus.Submitted).ToList();

                var stats = new
                {
                    TotalAttempts = attempts.Count,
                    CompletedAttempts = submittedAttempts.Count,
                    InProgressAttempts = attempts.Count(a => a.Status == QuizAttemptStatus.InProgress),
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

        public async Task<ServiceResponse<List<object>>> GetQuizScoresAsync(int quizId)
        {
            var response = new ServiceResponse<List<object>>();

            try
            {
                var attempts = await _quizAttemptRepository.GetByQuizIdAsync(quizId);

                // Chỉ lấy các attempt đã submit
                var submittedAttempts = attempts
                    .Where(a => a.Status == QuizAttemptStatus.Submitted)
                    .OrderByDescending(a => a.TotalScore)
                    .Select(a => new
                    {
                        AttemptId = a.AttemptId,
                        UserId = a.UserId,
                        UserName = $"{a.User?.FirstName} {a.User?.LastName}".Trim(),
                        AttemptNumber = a.AttemptNumber,
                        TotalScore = a.TotalScore,
                        Percentage = CalculatePercentage(a, a.Quiz),
                        IsPassed = a.Quiz?.PassingScore.HasValue == true ? a.TotalScore >= a.Quiz.PassingScore.Value : false,
                        SubmittedAt = a.SubmittedAt,
                        TimeSpentSeconds = a.TimeSpentSeconds
                    })
                    .ToList<object>();

                response.Success = true;
                response.Data = submittedAttempts;
                response.Message = $"Found {submittedAttempts.Count} completed attempts for quiz {quizId}";
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
        public async Task<ServiceResponse<PagedResult<object>>> GetQuizScoresPagedAsync(int quizId, PageRequest request)
        {
            var response = new ServiceResponse<PagedResult<object>>();

            try
            {
                var pagedResult = await _quizAttemptRepository.GetQuizScoresPagedAsync(quizId, request);

                var mappedItems = pagedResult.Items.Select(a => new
                {
                    AttemptId = a.AttemptId,
                    UserId = a.UserId,
                    UserName = $"{a.User?.FirstName} {a.User?.LastName}".Trim(),
                    AttemptNumber = a.AttemptNumber,
                    TotalScore = a.TotalScore,
                    Percentage = CalculatePercentage(a, a.Quiz),
                    IsPassed = a.Quiz?.PassingScore.HasValue == true ? a.TotalScore >= a.Quiz.PassingScore.Value : false,
                    SubmittedAt = a.SubmittedAt,
                    TimeSpentSeconds = a.TimeSpentSeconds
                }).ToList<object>();

                var result = new PagedResult<object>
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

        public async Task<ServiceResponse<List<QuizAttempt>>> GetUserQuizAttemptsAsync(int userId, int quizId)
        {
            var response = new ServiceResponse<List<QuizAttempt>>();

            try
            {
                var attempts = await _quizAttemptRepository.GetByUserAndQuizAsync(userId, quizId);
                response.Success = true;
                response.Data = attempts.OrderByDescending(a => a.StartedAt).ToList();
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
