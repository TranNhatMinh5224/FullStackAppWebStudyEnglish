using LearningEnglish.Application.Common.Enums;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using System.Linq.Expressions;

namespace LearningEnglish.Infrastructure.Services
{
    // Service sắp xếp cho User entity
    // Sử dụng dictionary để tuân thủ nguyên tắc Open/Closed Principle
    public class UserSortingService : ISortingService<User>
    {
        private readonly Dictionary<string, Expression<Func<User, object>>> _sortExpressions;

        public UserSortingService()
        {
            // Dictionary mapping sort field names to lambda expressions
            _sortExpressions = new Dictionary<string, Expression<Func<User, object>>>(StringComparer.OrdinalIgnoreCase)
            {
                { "email", u => u.Email },
                { "firstname", u => u.FirstName ?? string.Empty },
                { "lastname", u => u.LastName ?? string.Empty },
                { "createdat", u => u.CreatedAt },
                { "updatedat", u => u.UpdatedAt },
                { "status", u => u.Status }
            };
        }

        public IQueryable<User> ApplySort(IQueryable<User> query, string? sortBy, SortOrder sortOrder)
        {
            // Default sorting by CreatedAt Descending if no sort field specified
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return query.OrderByDescending(u => u.CreatedAt);
            }

            // Get the sort expression from dictionary
            if (_sortExpressions.TryGetValue(sortBy, out var sortExpression))
            {
                return sortOrder == SortOrder.Ascending
                    ? query.OrderBy(sortExpression)
                    : query.OrderByDescending(sortExpression);
            }

            // Fallback to default sorting if invalid sortBy provided
            return query.OrderByDescending(u => u.CreatedAt);
        }
    }
}
