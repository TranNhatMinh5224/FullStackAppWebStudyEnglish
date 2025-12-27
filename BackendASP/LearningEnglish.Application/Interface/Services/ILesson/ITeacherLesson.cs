using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface.Services.ILesson
{
    public interface ITeacherLessonService
    {
        // Teacher tạo lesson
        Task<ServiceResponse<LessonDto>> TeacherAddLesson(TeacherCreateLessonDto dto, int teacherId);
        
        // Teacher cập nhật lesson
        Task<ServiceResponse<LessonDto>> UpdateLesson(int lessonId, UpdateLessonDto dto);
        
        // Teacher xóa lesson
        Task<ServiceResponse<bool>> DeleteLesson(int lessonId);
        
        // Teacher lấy lesson theo ID (read-only)
        Task<ServiceResponse<LessonDto>> GetLessonById(int lessonId);
        // Teacher lấy danh sách lesson theo courseId
        Task<ServiceResponse<List<LessonDto>>> GetListLessonByCourseId(int courseId);
    }
}
