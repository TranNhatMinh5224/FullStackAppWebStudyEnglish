using LearningEnglish.Application.Common.Enums;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using System.Linq.Expressions;

namespace LearningEnglish.Infrastructure.Services;

// Service sắp xếp cho Course entity
// Tuân thủ nguyên tắc Single Responsibility và Open-Closed Principle
public class CourseSortingService : ISortingService<Course>
{
    private readonly Dictionary<string, Expression<Func<Course, object>>> _sortExpressions;

    public CourseSortingService()
    {
        // Dictionary-based sorting fields for Open-Closed Principle
        // Adding new sort fields doesn't require modifying existing code
#pragma warning disable CS8603 // Possible null reference return
        _sortExpressions = new Dictionary<string, Expression<Func<Course, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            { "title", c => c.Title! },
            { "createdat", c => c.CreatedAt },
            { "price", c => c.Price },
            { "enrollmentcount", c => c.EnrollmentCount },
            { "status", c => c.Status! },
            { "type", c => c.Type },
            { "isfeatured", c => c.IsFeatured },
            { "updatedat", c => c.UpdatedAt }
        };

    }

    public IQueryable<Course> ApplySort(IQueryable<Course> query, string? sortBy, SortOrder sortOrder)
    {
      
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderByDescending(c => c.CreatedAt)!;
        }

       
        if (!_sortExpressions.TryGetValue(sortBy, out var sortExpression))
        {
           
            return query.OrderByDescending(c => c.CreatedAt)!;
        }

        return sortOrder == SortOrder.Ascending
            ? query.OrderBy(sortExpression!)!
            : query.OrderByDescending(sortExpression!)!;
    }
}
