using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Interface
{
    public interface ICourseQueryService
    {
        Task<ServiceResponse<CourseDetailResponseDto>> GetCourseDetailAsync(int courseId, int? userId = null);
    }
}
