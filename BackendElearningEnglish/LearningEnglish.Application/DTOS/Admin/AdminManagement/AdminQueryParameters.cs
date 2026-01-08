using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.Application.DTOs.Admin;

/// <summary>
/// Query parameters cho danh sách admins
/// </summary>
public class AdminQueryParameters : PageRequest
{
    public string? SearchTerm { get; set; } // Tìm theo email, name
}
