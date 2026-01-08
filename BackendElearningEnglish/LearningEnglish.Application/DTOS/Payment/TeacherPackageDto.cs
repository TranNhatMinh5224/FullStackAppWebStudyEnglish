using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.DTOs
{
    public class CreateTeacherPackageDto
    {
        public string PackageName { get; set; } = string.Empty;
        public PackageLevel Level { get; set; }
        public decimal Price { get; set; }
        public int MaxCourses { get; set; }
        public int MaxLessons { get; set; }
        public int MaxStudents { get; set; }
    }
    public class TeacherPackageDto
    {
        public int TeacherPackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public PackageLevel Level { get; set; }
        public decimal Price { get; set; }
        public int MaxCourses { get; set; }
        public int MaxLessons { get; set; }
        public int MaxStudents { get; set; }
    }

    // DTO cập nhật gói giáo viên - Hỗ trợ PARTIAL UPDATE
    public class UpdateTeacherPackageDto
    {
        // Nullable để phân biệt: không gửi vs gửi giá trị
        public string? PackageName { get; set; }
        public PackageLevel? Level { get; set; }
        public decimal? Price { get; set; }
        public int? MaxCourses { get; set; }
        public int? MaxLessons { get; set; }
        public int? MaxStudents { get; set; }
    }

}
