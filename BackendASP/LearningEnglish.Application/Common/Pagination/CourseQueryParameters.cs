using LearningEnglish.Application.Common.Enums;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Common.Pagination
{
    // Query parameters cho Course - kế thừa từ PageRequest
    // Tuân thủ Single Responsibility: chễ lo về course query parameters
    public class CourseQueryParameters : PageRequest
    {
        // Search
        public string? SearchTerm { get; set; }
        
        // Filters
        public CourseType? Type { get; set; }
        public CourseStatus? Status { get; set; }
        public bool? IsFeatured { get; set; }
        public int? TeacherId { get; set; }
        
        // Sorting
        public string? SortBy { get; set; }
        public SortOrder SortOrder { get; set; } = SortOrder.Ascending;
    }
}
