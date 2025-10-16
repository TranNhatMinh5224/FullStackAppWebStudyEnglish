using CleanDemo.Application.Common;
using CleanDemo.Application.DTOs;

namespace CleanDemo.Application.Interface
{
    public interface ILessonService
    {
        // CRUD với ServiceResponse và DTOs
        Task<ServiceResponse<List<ListLessonDto>>> GetListLessonByCourseId(int CourseId);
        Task<ServiceResponse<LessonDto>> GetLessonById(int lessonId);
        Task<ServiceResponse<LessonDto>> AdminAddLesson(AdminCreateLessonDto dto);
        Task<ServiceResponse<LessonDto>> TeacherAddLesson(TeacherCreateLessonDto dto);
        Task<ServiceResponse<LessonDto>> UpdateLesson(UpdateLessonDto dto);
        Task<ServiceResponse<bool>> DeleteLesson(DeleteLessonDto dto);
    }
}