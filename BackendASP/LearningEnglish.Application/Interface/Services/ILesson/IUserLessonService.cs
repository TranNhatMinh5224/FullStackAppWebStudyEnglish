using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface.Services.ILesson
{
    /// <summary>
    /// User lesson service - For Student/User operations with progress tracking
    /// </summary>
    public interface ILessonService
    {
        // Lấy danh sách lesson theo courseId + Tiến độ học tập của User đó 
        Task<ServiceResponse<List<LessonWithProgressDto>>> GetListLessonByCourseId(int courseId, int userId);
        
        // Lấy chi tiết lesson + Tiến độ học tập của User đó
        Task<ServiceResponse<LessonWithProgressDto>> GetLessonById(int lessonId, int userId);
    }
}
