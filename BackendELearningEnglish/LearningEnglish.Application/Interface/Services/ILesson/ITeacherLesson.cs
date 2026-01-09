using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface.Services.Lesson
{
    public interface ITeacherLessonService
    {
        // Teacher tạo lesson
        Task<ServiceResponse<LessonDto>> TeacherAddLesson(TeacherCreateLessonDto dto, int teacherId);
        
        // Teacher cập nhật lesson
        Task<ServiceResponse<LessonDto>> UpdateLesson(int lessonId, UpdateLessonDto dto, int teacherId);
        
        // Teacher xóa lesson
        Task<ServiceResponse<bool>> DeleteLesson(int lessonId, int teacherId);
        
        // Teacher lấy lesson theo ID (read-only)
        Task<ServiceResponse<LessonDto>> GetLessonById(int lessonId, int teacherId);
        // Teacher lấy danh sách lesson theo courseId
        Task<ServiceResponse<List<LessonDto>>> GetListLessonByCourseId(int courseId, int teacherId);
    }
}
