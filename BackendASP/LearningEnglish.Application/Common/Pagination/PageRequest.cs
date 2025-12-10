namespace LearningEnglish.Application.Common.Pagination
{
    // Pagination request DTO
    public class PageRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
    }
}
