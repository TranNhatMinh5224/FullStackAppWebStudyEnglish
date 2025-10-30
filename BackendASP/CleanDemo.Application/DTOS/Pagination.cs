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
        public int TotalItems { get; set; } // tổng số bản ghi thỏa mãn điều kiện 

        public int PageNumber { get; set; } // trang mà người dùng yêu cầu
        public int PageSize { get; set; } // số bản ghi trên mỗi trang
        public int TotalPages { get; set; } // tổng số trang = TotalItems / PageSize
    }
}