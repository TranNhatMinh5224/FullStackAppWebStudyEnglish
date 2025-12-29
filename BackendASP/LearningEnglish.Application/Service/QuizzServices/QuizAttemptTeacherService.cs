using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
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
        private readonly IMapper _mapper;
        private readonly ILogger<QuizAttemptTeacherService> _logger;

        public QuizAttemptTeacherService(
            IQuizAttemptRepository quizAttemptRepository,
            IQuizRepository quizRepository,
            IAssessmentRepository assessmentRepository,
            IModuleRepository moduleRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<QuizAttemptTeacherService> logger)
        {
            _quizAttemptRepository = quizAttemptRepository;
            _quizRepository = quizRepository;
            _assessmentRepository = assessmentRepository;
            _moduleRepository = moduleRepository;
            _courseRepository = courseRepository;
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

        public async Task<ServiceResponse<QuizAttemptDto>> GetAttemptDetailsAsync(int attemptId, int teacherId)
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

                // Check ownership/enrollment for the quiz
                var accessCheck = await CheckTeacherAccessToQuizAsync(attempt.QuizId, teacherId);
                if (!accessCheck.Success)
                {
                    response.Success = false;
                    response.StatusCode = accessCheck.StatusCode;
                    response.Message = accessCheck.Message;
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
                _logger.LogError(ex, "Error getting attempt details {AttemptId} for teacher {TeacherId}", attemptId, teacherId);
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

