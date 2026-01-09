using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface.Services.Lecture
{
    public interface ITeacherLectureQueryService
    {
        // Teacher lấy lecture theo ID
        Task<ServiceResponse<LectureDto>> GetLectureByIdAsync(int lectureId, int teacherId);

        // Teacher lấy danh sách lecture theo module
        Task<ServiceResponse<List<ListLectureDto>>> GetLecturesByModuleIdAsync(int moduleId, int teacherId);

        // Teacher lấy cây lecture
        Task<ServiceResponse<List<LectureTreeDto>>> GetLectureTreeByModuleIdAsync(int moduleId, int teacherId);
    }
}