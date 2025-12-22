using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface ILessonService
    {
        // Lấy danh sách lesson của course
        Task<ServiceResponse<List<LessonWithProgressDto>>> GetListLessonByCourseId(int CourseId, int userId, string userRole);
        
        // Lấy thông tin lesson
        Task<ServiceResponse<LessonDto>> GetLessonById(int lessonId, int userId, string userRole);
        
        // Admin tạo lesson
        Task<ServiceResponse<LessonDto>> AdminAddLesson(AdminCreateLessonDto dto);
        
        // Teacher tạo lesson
        Task<ServiceResponse<LessonDto>> TeacherAddLesson(TeacherCreateLessonDto dto, int userId);
        
        // Cập nhật lesson
        Task<ServiceResponse<LessonDto>> UpdateLesson(int lessonId, UpdateLessonDto dto);
        
        // Xóa lesson
        Task<ServiceResponse<bool>> DeleteLesson(int lessonId);
        Task<ServiceResponse<bool>> DeleteLesson(DeleteLessonDto dto);

        // Xóa lesson có kiểm tra quyền
        Task<ServiceResponse<bool>> DeleteLessonWithAuthorizationAsync(int lessonId, int userId, string userRole);
        
        // Cập nhật lesson có kiểm tra quyền
        Task<ServiceResponse<LessonDto>> UpdateLessonWithAuthorizationAsync(int lessonId, UpdateLessonDto dto, int userId, string userRole);
    }
}
