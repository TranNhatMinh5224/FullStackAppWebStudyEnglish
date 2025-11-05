using CleanDemo.Domain.Entities;
namespace CleanDemo.Application.Interface
{
    public interface IMiniTestRepository
    {
        Task AddMiniTestAsync(MiniTest miniTest);
        Task<bool> MiniTestExistsInLesson(string title, int lessonId);
        Task<List<MiniTest>?> GetListMiniTestByIdLesson(int lessonId);
       
    }
}