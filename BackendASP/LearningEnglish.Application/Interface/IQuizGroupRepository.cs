using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizGroupRepository
    {
        Task<QuizGroup?> GetQuizGroupByIdAsync(int quizGroupId);
        Task<List<QuizGroup>> GetQuizGroupsByQuizSectionIdAsync(int quizSectionId);
        Task<QuizGroup> CreateQuizGroupAsync(QuizGroup quizGroup);
        Task<QuizGroup> UpdateQuizGroupAsync(QuizGroup quizGroup);
        Task<bool> DeleteQuizGroupAsync(int quizGroupId);
        Task<bool> QuizGroupExistsAsync(int quizGroupId);
        Task<QuizSection?> GetQuizSectionByIdAsync(int quizSectionId);
    }
}
