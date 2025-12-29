using LearningEnglish.Application.Common.Enums;

namespace LearningEnglish.Application.Common.Pagination;

// Tham số query cho User với hỗ trợ tìm kiếm, sắp xếp và phân trang
public class UserQueryParameters : PageRequest
{
    // Từ khóa tìm kiếm (áp dụng cho Email, FirstName, LastName)
    public string? SearchTerm { get; set; }

    // Tên trường để sắp xếp (vd: "email", "firstname", "lastname", "createdat")
    public string? SortBy { get; set; }

    // Thứ tự sắp xếp: Tăng dần (1) hoặc Giảm dần (2)
    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;
}
