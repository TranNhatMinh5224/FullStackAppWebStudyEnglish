using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Service
{
    public class QuizAttemptService : IQuizAttemptService
    {
        // TODO: Implement methods
        public Task<ServiceResponse<StartQuizAttemptResponseDto>> StartAttemptAsync(int userId, StartQuizAttemptRequestDto request)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<bool>> UpdateAnswerAsync(int userId, int attemptId, UpdateAnswerDto answerDto)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<QuizAttemptDto>> GetAttemptAsync(int userId, int attemptId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<QuizAttemptDto>> FinishAttemptAsync(int userId, int attemptId)
        {
            throw new NotImplementedException();
        }
    }
}