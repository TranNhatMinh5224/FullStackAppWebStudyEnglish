using LearningEnglish.Application.Common.Specifications;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LearningEnglish.Infrastructure.Extensions
{
    // Extension methods để apply Specification pattern vào IQueryable
    // Tuân thủ Single Responsibility: tách riêng specification logic
    public static class SpecificationExtensions
    {
        public static IQueryable<T> ApplySpecification<T>(
            this IQueryable<T> query,
            ISpecification<T> spec) where T : class
        {
            // Apply criteria (WHERE clause)
            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            // Apply includes (eager loading)
            query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
            
            // Apply include strings (for nested includes)
            query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

            // Apply OrderBy
            if (spec.OrderBy != null)
            {
                query = query.OrderBy(spec.OrderBy);
            }
            else if (spec.OrderByDescending != null)
            {
                query = query.OrderByDescending(spec.OrderByDescending);
            }

            // Apply tracking behavior
            if (spec.AsNoTracking)
            {
                query = query.AsNoTracking();
            }

            return query;
        }
    }
}
