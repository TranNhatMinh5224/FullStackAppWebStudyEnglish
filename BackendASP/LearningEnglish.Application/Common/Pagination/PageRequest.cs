namespace LearningEnglish.Application.Common.Pagination
{
    // Pagination request DTO with validation
    public class PageRequest
    {
        private int _pageNumber = 1;
        private int _pageSize = 20;
        
        // Trang bao nhiêu: mặc định trang đầu tiên, tối thiểu là 1
        public int PageNumber 
        { 
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }
        
        // Số lượng bản ghi của mỗi trang: mặc định 20, tối thiểu 1, tối đa 100
        public int PageSize 
        { 
            get => _pageSize;
            set => _pageSize = value switch
            {
                < 1 => 20,      // Nếu < 1 thì dùng default
                > 100 => 100,   // Max 100 items per page để tránh overload
                _ => value
            };
        }
        
        public string? SearchTerm { get; set; } // từ khóa tìm kiếm
    }
}
