using LearningEnglish.Application.Common;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface ILessonProgressService
    {
        Task<ServiceResponse<bool>> UpdateLessonProgressAsync(int userId, int lessonId);
    }
}