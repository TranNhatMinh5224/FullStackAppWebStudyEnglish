using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface ILectureRepository
    {
        // Basic CRUD operations
        Task<Lecture?> GetByIdAsync(int lectureId);
        Task<Lecture?> GetByIdWithDetailsAsync(int lectureId);
        Task<List<Lecture>> GetByModuleIdAsync(int moduleId);
        Task<List<Lecture>> GetByModuleIdWithDetailsAsync(int moduleId);
        Task<Lecture> CreateAsync(Lecture lecture);
        Task<Lecture> UpdateAsync(Lecture lecture);
        Task<bool> DeleteAsync(int lectureId);
        Task<bool> ExistsAsync(int lectureId);

        // Tree structure operations
        Task<List<Lecture>> GetChildrenAsync(int parentLectureId);
        Task<List<Lecture>> GetTreeByModuleIdAsync(int moduleId);
        Task<bool> HasChildrenAsync(int lectureId);
        Task<bool> IsValidParentAsync(int lectureId, int? parentLectureId);

        // Helper operations
        Task<int> GetMaxOrderIndexAsync(int moduleId, int? parentLectureId = null);
        
        // Authorization helpers
        Task<Lecture?> GetLectureWithModuleCourseAsync(int lectureId);
    }
}
