using System.Linq.Expressions;

namespace LearningEnglish.Application.Common.Specifications
{
    // Interface định nghĩa Specification pattern để tách biệt query logic khỏi repository
    // Tuân thủ Open-Closed Principle: mở rộng bằng cách thêm specification mới
    public interface ISpecification<T>
    {
        // Criteria để filter
        Expression<Func<T, bool>>? Criteria { get; }
        
        // Danh sách includes cho eager loading
        List<Expression<Func<T, object>>> Includes { get; }
        
        // Include strings cho nested navigation properties
        List<string> IncludeStrings { get; }
        
        // OrderBy expression
        Expression<Func<T, object>>? OrderBy { get; }
        
        // OrderByDescending expression
        Expression<Func<T, object>>? OrderByDescending { get; }
        
        // Enable tracking (default: false for read-only queries)
        bool AsNoTracking { get; }
    }
}
