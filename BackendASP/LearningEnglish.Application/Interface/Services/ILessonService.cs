using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface ILessonService
    {
        // Lấy danh sách lesson của course (RLS đã filter theo ownership/enrollment)
        // userId optional: chỉ cần khi tính progress cho Student
        Task<ServiceResponse<List<LessonWithProgressDto>>> GetListLessonByCourseId(int courseId, int? userId = null);
        
        // Lấy thông tin lesson (RLS đã filter theo ownership/enrollment)
        Task<ServiceResponse<LessonDto>> GetLessonById(int lessonId);
        
        // Admin tạo lesson
        Task<ServiceResponse<LessonDto>> AdminAddLesson(AdminCreateLessonDto dto);
        
        // Teacher tạo lesson
        Task<ServiceResponse<LessonDto>> TeacherAddLesson(TeacherCreateLessonDto dto, int teacherId);
        
        // Cập nhật lesson (RLS đã filter theo ownership)
        Task<ServiceResponse<LessonDto>> UpdateLesson(int lessonId, UpdateLessonDto dto);
        
        // Xóa lesson (RLS đã filter theo ownership)
        Task<ServiceResponse<bool>> DeleteLesson(int lessonId);
    }
}
