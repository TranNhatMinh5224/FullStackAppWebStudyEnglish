using LearningEnglish.Domain.Enums;
namespace LearningEnglish.Application.DTOs
{
    public class PurchaseTeacherPackageDto
    {
        public int IdTeacherPackage { get; set; }
    }
    public class ResPurchaseTeacherPackageDto
    {
        public int Id { get; set; }
        public int IdTeacherPackage { get; set; }
        public int IdUser { get; set; }
        public string UserName { get; set; } = string.Empty;

        public string PackageName { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class ListTeacherSubcription
    {
        public int TeacherSubscriptionId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public SubscriptionStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class DeleteTeacherSubscriptionDto
    {
        public int TeacherSubscriptionId { get; set; }
    }
}
