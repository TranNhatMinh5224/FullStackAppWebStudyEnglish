using CleanDemo.Domain.Enums;

namespace CleanDemo.Application.DTOs
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
    public class TeacherPackageDetailDto : TeacherPackageDto
    {
        public int DurationMonths { get; set; }
    }
    public class UpdateTeacherPackageDto : CreateTeacherPackageDto
    {

    }

}