using LearningEnglish.Application.Common;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface IModuleProgressService
    {
        Task<ServiceResponse<bool>> StartModuleAsync(int userId, int moduleId);
        Task<ServiceResponse<bool>> CompleteModuleAsync(int userId, int moduleId);
    }
}