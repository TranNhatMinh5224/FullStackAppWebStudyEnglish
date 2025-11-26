using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface ICourseProgressService
    {
        Task<ServiceResponse<CourseProgressDetailDto>> GetCourseProgressDetailAsync(int userId, int courseId);
        Task<ServiceResponse<bool>> UpdateCourseProgressAsync(int userId, int courseId);
    }
}