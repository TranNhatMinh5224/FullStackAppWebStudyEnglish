using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface.Services.Lecture
{
    public interface IUserLectureService
    {
        // Lấy thông tin lecture với progress của user
        Task<ServiceResponse<LectureDto>> GetLectureByIdAsync(int lectureId, int userId);
        
        // Lấy danh sách lecture theo module với progress của user
        Task<ServiceResponse<List<ListLectureDto>>> GetLecturesByModuleIdAsync(int moduleId, int userId);
        
        // Lấy cây lecture với progress của user
        Task<ServiceResponse<List<LectureTreeDto>>> GetLectureTreeByModuleIdAsync(int moduleId, int userId);
    }
}
