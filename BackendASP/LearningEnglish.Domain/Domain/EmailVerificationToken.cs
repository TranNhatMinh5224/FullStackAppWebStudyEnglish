namespace LearningEnglish.Domain.Entities;

public class EmailVerificationToken
{
    public int EmailVerificationTokenId { get; set; } // Primary Key
    public int UserId { get; set; } // Foreign Key
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }


    public int AttemptsCount { get; set; } = 0;


    public bool IsUsed { get; set; } = false;

    // Navigation property
    public User? User { get; set; }

    // Helper method
    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
}
