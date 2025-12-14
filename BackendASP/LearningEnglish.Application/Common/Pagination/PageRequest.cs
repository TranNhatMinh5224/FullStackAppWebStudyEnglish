namespace LearningEnglish.Application.Common.Pagination
{
    // Pagination request DTO
    public class PageRequest
    {
        public int PageNumber { get; set; } = 1; // Trang bao nhiêu : mặc định trang đầu tiên 
        public int PageSize { get; set; } = 20; // Số lượng bản ghi của mỗi trang  mặc định số lượng bản ghi trên mỗi trang
        public string? SearchTerm { get; set; } // từ khóa tìm kiếm
    }
}
