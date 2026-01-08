using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface ILectureRepository
    {
        // Lấy bài giảng theo ID
        Task<Lecture?> GetByIdAsync(int lectureId);
        
        // Lấy bài giảng theo ID cho Teacher (kiểm tra ownership qua course)
        Task<Lecture?> GetByIdForTeacherAsync(int lectureId, int teacherId);
        
        // Lấy bài giảng với chi tiết
        Task<Lecture?> GetByIdWithDetailsAsync(int lectureId);
        
        // Lấy bài giảng theo module
        Task<List<Lecture>> GetByModuleIdAsync(int moduleId);
        
        // Lấy bài giảng theo module với chi tiết
        Task<List<Lecture>> GetByModuleIdWithDetailsAsync(int moduleId);
        
        // Tạo bài giảng
        Task<Lecture> CreateAsync(Lecture lecture);
        
        // Cập nhật bài giảng
        Task<Lecture> UpdateAsync(Lecture lecture);
        
        // Xóa bài giảng
        Task<bool> DeleteAsync(int lectureId);
        
        // Kiểm tra bài giảng tồn tại
        Task<bool> ExistsAsync(int lectureId);

        // Lấy cây bài giảng theo module
        Task<List<Lecture>> GetTreeByModuleIdAsync(int moduleId);
        
        // Kiểm tra có bài giảng con
        Task<bool> HasChildrenAsync(int lectureId);

        // Lấy order index lớn nhất
        Task<int> GetMaxOrderIndexAsync(int moduleId, int? parentLectureId = null);

        // Lấy bài giảng với module và course
        Task<Lecture?> GetLectureWithModuleCourseAsync(int lectureId);

        // Lấy bài giảng với module và course cho Teacher (kiểm tra ownership)
        Task<Lecture?> GetLectureWithModuleCourseForTeacherAsync(int lectureId, int teacherId);
    }
}
