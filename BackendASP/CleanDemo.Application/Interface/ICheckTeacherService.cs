using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Interface
{
    public interface ICheckTeacherService
    {
        Task<ServiceResponse<ResCheckCreateCourse>> CheckTeacherCreateCourseAsync(int teacherId);

    }

}
