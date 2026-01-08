using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Application.Common.Pagination
{
    // static class chứa các phương thức mở rộng cho pagination
    public static class PaginationExtensions
    {     
        // xử lý phân trang cho một truy vấn  đầu vào là : trang số N và kích thước trang
        public static async Task<PagedResult<T>> ToPagedListAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize)
        {
            var count = await query.CountAsync(); // tổng số bản ghi trong truy vấn
            var items = await query // lấy danh sách bản ghi trên trang hiện tại
                .Skip((pageNumber - 1) * pageSize) // bỏ qua các bản ghi của các trang trước
                .Take(pageSize) // lấy số bản ghi của trang hiện tại    
                .ToListAsync(); // chuyển đổi sang danh sách

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = count,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public static async Task<PagedResult<T>> ToPagedListAsync<T>(
            this IQueryable<T> query,
            PageRequest request)
        {
            return await query.ToPagedListAsync(request.PageNumber, request.PageSize);
        }
    }
}
