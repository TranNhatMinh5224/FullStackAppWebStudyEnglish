using CleanDemo.Domain.Enums;

namespace CleanDemo.Domain.Entities;

public class TeacherSubscription
{
    public int TeacherSubscriptionId { get; set; }
    public int UserId { get; set; }
    public int TeacherPackageId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;
    public bool? AutoRenew { get; set; } = false;


    public int? PaymentId { get; set; }
    public Payment? Payment { get; set; }

    public User User { get; set; } = null!;
    public TeacherPackage TeacherPackage { get; set; } = null!;

}
