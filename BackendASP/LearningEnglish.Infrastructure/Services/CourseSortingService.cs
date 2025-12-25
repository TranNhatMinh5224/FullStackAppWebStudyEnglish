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
        _sortExpressions = new Dictionary<string, Expression<Func<Course, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            { "title", c => c.Title },
            { "createdat", c => c.CreatedAt },
            { "price", c => c.Price },
            { "enrollmentcount", c => c.EnrollmentCount },
            { "status", c => c.Status },
            { "type", c => c.Type },
            { "isfeatured", c => c.IsFeatured },
            { "updatedat", c => c.UpdatedAt }
        };
    }

    public IQueryable<Course> ApplySort(IQueryable<Course> query, string? sortBy, SortOrder sortOrder)
    {
        // If no sort field specified, default to CreatedAt descending
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderByDescending(c => c.CreatedAt);
        }

        // Check if sort field exists in dictionary
        if (!_sortExpressions.TryGetValue(sortBy, out var sortExpression))
        {
            // If invalid sort field, fall back to default
            return query.OrderByDescending(c => c.CreatedAt);
        }

        // Apply sorting based on SortOrder enum
        return sortOrder == SortOrder.Ascending
            ? query.OrderBy(sortExpression!)
            : query.OrderByDescending(sortExpression!);
    }
}
