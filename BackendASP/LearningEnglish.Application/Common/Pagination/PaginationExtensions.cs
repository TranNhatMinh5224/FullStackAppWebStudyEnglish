using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Application.Common.Pagination
{
    // Extension methods cho pagination
    public static class PaginationExtensions
    {
        public static async Task<PagedResult<T>> ToPagedListAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize)
        {
            var count = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

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
