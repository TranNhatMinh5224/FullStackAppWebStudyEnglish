using CleanDemo.Domain.Enums;

namespace CleanDemo.Domain.Entities;

public class Payment
{
    public int PaymentId { get; set; }
    public int UserId { get; set; }
    public int? CourseId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public User? User { get; set; }
    public Course? Course { get; set; }
}
