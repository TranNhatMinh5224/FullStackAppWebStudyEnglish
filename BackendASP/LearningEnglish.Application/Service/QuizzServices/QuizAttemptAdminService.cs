using AutoMapper;
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
        private readonly IMapper _mapper;

        public QuizAttemptAdminService(
            IQuizAttemptService quizAttemptService,
            IQuizAttemptRepository quizAttemptRepository,
            IMapper mapper)
        {
            _quizAttemptService = quizAttemptService;
            _quizAttemptRepository = quizAttemptRepository;
            _mapper = mapper;
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

        public async Task<ServiceResponse<QuizAttemptDto>> GetAttemptDetailsAsync(int attemptId)
        {
            var response = new ServiceResponse<QuizAttemptDto>();

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

                var attemptDto = _mapper.Map<QuizAttemptDto>(attempt);

                response.Success = true;
                response.Data = attemptDto;
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
