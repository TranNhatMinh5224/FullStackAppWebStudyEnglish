using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizSectionRepository
    {
        Task<QuizSection?> GetQuizSectionByIdAsync(int quizSectionId);
        Task<List<QuizSection>> GetQuizSectionsByQuizIdAsync(int quizId);
        Task<QuizSection> CreateQuizSectionAsync(QuizSection quizSection);
        Task<QuizSection> UpdateQuizSectionAsync(QuizSection quizSection);
        Task<bool> DeleteQuizSectionAsync(int quizSectionId);
        Task<bool> QuizSectionExistsAsync(int quizSectionId);
        Task<Quiz?> GetQuizByIdAsync(int quizId);
    }
}
