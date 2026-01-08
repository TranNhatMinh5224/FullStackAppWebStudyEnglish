using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Common.Pagination
{
    // Query parameters cho Admin lấy danh sách khóa học
    // Không có sortBy/sortOrder - luôn sort theo Title mặc định
    public class AdminCourseQueryParameters : PageRequest
    {
        // Search
        public string? SearchTerm { get; set; }
        
        // Filters
        public CourseType? Type { get; set; }
        public CourseStatus? Status { get; set; }
        public bool? IsFeatured { get; set; }
        public int? TeacherId { get; set; }
    }
}

