using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IModuleRepository
    {
        // Lấy module theo ID
        Task<Module?> GetByIdAsync(int moduleId);
        
        // Lấy module với chi tiết
        Task<Module?> GetByIdWithDetailsAsync(int moduleId);
        
        // Lấy module theo lesson
        Task<List<Module>> GetByLessonIdAsync(int lessonId);
        
        // Lấy module theo lesson với chi tiết
        Task<List<Module>> GetByLessonIdWithDetailsAsync(int lessonId);
        
        // Tạo module
        Task<Module> CreateAsync(Module module);
        
        // Cập nhật module
        Task<Module> UpdateAsync(Module module);
        
        // Xóa module
        Task<bool> DeleteAsync(int moduleId);
        
        // Kiểm tra module tồn tại
        Task<bool> ExistsAsync(int moduleId);

        // Lấy order index lớn nhất
        Task<int> GetMaxOrderIndexAsync(int lessonId);
        
        // Lấy lesson ID từ module ID
        Task<int?> GetLessonIdByModuleIdAsync(int moduleId);

        // Lấy module với course
        Task<Module?> GetModuleWithCourseAsync(int moduleId);
    }
}
