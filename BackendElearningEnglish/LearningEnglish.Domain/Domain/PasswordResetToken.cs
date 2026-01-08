namespace LearningEnglish.Domain.Entities;

public class PasswordResetToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Anti-spam fields
    public int AttemptsCount { get; set; } = 0;  // Số lần nhập sai OTP
    public DateTime? BlockedUntil { get; set; }   // Thời điểm hết block (null = không bị block)

    // Navigation Properties
    public User? User { get; set; }
}
