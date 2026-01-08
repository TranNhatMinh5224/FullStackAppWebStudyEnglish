namespace LearningEnglish.Application.Common.Pagination
{
    // Pagination response DTO
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new(); // danh sách bản ghi trên trang hiện tại
        public int TotalCount { get; set; } //  tổng số bản ghi
        public int PageNumber { get; set; } = 1; // trang hiện tại
        public int PageSize { get; set; } = 20; // số lượng bản ghi trên mỗi trang
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize); // tổng số trang
        public bool HasPrevious => PageNumber > 1; // có trang trước không
        public bool HasNext => PageNumber < TotalPages; // có trang sau không
    }
}
