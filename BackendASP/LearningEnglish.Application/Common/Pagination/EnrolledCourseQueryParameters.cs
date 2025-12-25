using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Common.Pagination
{
    // Query parameters cho enrolled courses của User
    // Kế thừa PageRequest, thêm filter theo Type
    public class EnrolledCourseQueryParameters : PageRequest
    {
        // Lọc theo loại khóa học: System, Teacher, hoặc null = tất cả
        public CourseType? Type { get; set; }
    }
}
