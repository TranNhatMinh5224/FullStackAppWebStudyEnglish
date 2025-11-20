using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

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

                var stats = new
                {
                    TotalAttempts = attempts.Count,
                    CompletedAttempts = attempts.Count(a => a.Status == QuizAttemptStatus.Submitted),
                    InProgressAttempts = attempts.Count(a => a.Status == QuizAttemptStatus.InProgress),
                    AverageScore = attempts.Where(a => a.Status == QuizAttemptStatus.Submitted).Average(a => a.TotalScore),
                    HighestScore = attempts.Where(a => a.Status == QuizAttemptStatus.Submitted).Max(a => a.TotalScore),
                    LowestScore = attempts.Where(a => a.Status == QuizAttemptStatus.Submitted).Min(a => a.TotalScore)
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
                        Percentage = a.Quiz?.TotalPossibleScore > 0 ? (a.TotalScore / a.Quiz.TotalPossibleScore) * 100 : 0,
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
    }
}
