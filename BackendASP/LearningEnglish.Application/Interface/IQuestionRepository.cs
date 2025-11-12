using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IQuestionRepository
    {
        Task AddQuestionAsync(Question question);
        Task AddBulkQuestionsAsync(List<Question> questions);
        Task<List<int>> AddBulkQuestionsWithTransactionAsync(List<Question> questions);
        Task<Question?> GetQuestionByIdAsync(int questionId);
        Task<List<Question>> GetQuestionsByQuizGroupIdAsync(int quizGroupId);
        Task<List<Question>> GetQuestionsByQuizSectionIdAsync(int quizSectionId);
        Task UpdateQuestionAsync(Question question);
        Task DeleteQuestionAsync(int questionId);
    }
}
