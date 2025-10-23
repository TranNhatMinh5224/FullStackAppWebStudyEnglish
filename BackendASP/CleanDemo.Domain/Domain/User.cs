// User.cs
using CleanDemo.Domain.Enums;

namespace CleanDemo.Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public StatusAccount Status { get; set; } = StatusAccount.Active;

    // Many-to-many
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Role> Roles { get; set; } = new List<Role>();

    // Các nav khác…
    public List<UserCourse> UserCourses { get; set; } = new();
    public List<Course> CreatedCourses { get; set; } = new();

    public List<TeacherSubscription> TeacherSubscriptions { get; set; } = new();
    public int? CurrentTeacherSubscriptionId { get; set; }
    public TeacherSubscription? CurrentTeacherSubscription { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = new();
    public List<PasswordResetToken> PasswordResetTokens { get; set; } = new();
    public List<Progress> ProgressRecords { get; set; } = new();
    public List<ReviewWord> ReviewWords { get; set; } = new();
    public List<PronunciationScore> PronunciationScores { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();

    public void SetPassword(string password) =>
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

    public bool VerifyPassword(string password) =>
        BCrypt.Net.BCrypt.Verify(password, PasswordHash);
}
