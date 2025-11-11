using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizRepository
    {
        Task AddQuizAsync(Quiz quiz);
        Task<Quiz?> GetQuizByIdAsync(int quizId);
        Task<List<Quiz>> GetQuizzesByAssessmentIdAsync(int assessmentId);
        Task UpdateQuizAsync(Quiz quiz);
        Task DeleteQuizAsync(int quizId);


    }
}