namespace CleanDemo.Application.DTOs
{
    public class paginationRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "id"; // Mặc định sắp xếp theo Id
        public bool SortDesc { get; set; } = false; // Mặc định sắp xếp tăng dần

    }
    public class paginationResponseDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}