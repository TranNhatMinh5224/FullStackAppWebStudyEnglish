namespace LearningEnglish.Domain.Entities;

public class EmailVerificationToken
{
    public int EmailVerificationTokenId { get; set; } // Primary Key
    public int UserId { get; set; } // Foreign Key
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;

    // Anti-spam fields
    public int AttemptsCount { get; set; } = 0;  // Số lần nhập sai OTP
    public DateTime? BlockedUntil { get; set; }   // Thời điểm hết block (null = không bị block)

    // Navigation property
    public User? User { get; set; }

    // Helper methods
    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
    public bool IsBlocked() => BlockedUntil.HasValue && DateTime.UtcNow < BlockedUntil.Value;
}
