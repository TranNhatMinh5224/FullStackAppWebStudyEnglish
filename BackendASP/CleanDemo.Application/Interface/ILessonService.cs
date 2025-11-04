using CleanDemo.Application.Common;
using CleanDemo.Application.DTOs;

namespace CleanDemo.Application.Interface
{
    public interface ILessonService
    {
        // CRUD với ServiceResponse và DTOs
        Task<ServiceResponse<List<ListLessonDto>>> GetListLessonByCourseId(int CourseId, int userId, string userRole);
        Task<ServiceResponse<LessonDto>> GetLessonById(int lessonId, int userId, string userRole);
        Task<ServiceResponse<LessonDto>> AdminAddLesson(AdminCreateLessonDto dto);
        Task<ServiceResponse<LessonDto>> TeacherAddLesson(TeacherCreateLessonDto dto);
        Task<ServiceResponse<LessonDto>> UpdateLesson(int lessonId, UpdateLessonDto dto);
        Task<ServiceResponse<bool>> DeleteLesson(int lessonId);
        Task<ServiceResponse<bool>> DeleteLesson(DeleteLessonDto dto);
        Task<bool> CheckTeacherLessonPermission(int lessonId, int teacherId);
        
        // Methods with authorization built-in
        Task<ServiceResponse<bool>> DeleteLessonWithAuthorizationAsync(int lessonId, int userId, string userRole);
        Task<ServiceResponse<LessonDto>> UpdateLessonWithAuthorizationAsync(int lessonId, UpdateLessonDto dto, int userId, string userRole);
    }
}