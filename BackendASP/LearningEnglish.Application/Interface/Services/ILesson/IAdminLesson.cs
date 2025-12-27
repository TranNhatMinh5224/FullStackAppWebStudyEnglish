using System.Runtime.CompilerServices;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface.Services.ILesson
{
    public interface IAdminLessonService
    {
        // Admin tạo lesson
        Task<ServiceResponse<LessonDto>> AdminAddLesson(AdminCreateLessonDto dto);
        
        // Admin cập nhật lesson
        Task<ServiceResponse<LessonDto>> UpdateLesson(int lessonId, UpdateLessonDto dto);
        
        // Admin xóa lesson
        Task<ServiceResponse<bool>> DeleteLesson(int lessonId);
        Task<ServiceResponse<List<LessonDto>>> GetListLessonByCourseId(int courseId);
        // Admin lấy lesson theo ID (read-only)
        Task<ServiceResponse<LessonDto>> GetLessonById(int lessonId);
    }
}
