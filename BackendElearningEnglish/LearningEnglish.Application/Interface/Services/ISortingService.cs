using LearningEnglish.Application.Common.Enums;

namespace LearningEnglish.Application.Interface
{
    // Interface cho sorting service
 
    public interface ISortingService<T>
    {
        IQueryable<T> ApplySort(IQueryable<T> query, string? sortBy, SortOrder sortOrder);
    }
}
