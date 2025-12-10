// User.cs
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Domain.Entities;

public class User
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public bool IsMale { get; set; } = true;

    // Computed property for display name
    public string DisplayName => $"{FirstName} {LastName}".Trim();
    public string FullName => $"{FirstName} {LastName}".Trim();


    public string NormalizedEmail { get; set; } = string.Empty;
    public bool EmailVerified { get; set; } = false;

    public string? PasswordHash { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;

    public string? AvatarKey { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public int? CurrentTeacherSubscriptionId { get; set; }

    public List<Role> Roles { get; set; } = new();
    public List<Course> CreatedCourses { get; set; } = new();
    public List<CourseProgress> CourseProgresses { get; set; } = new();
    public List<TeacherSubscription> TeacherSubscriptions { get; set; } = new();
    public TeacherSubscription? CurrentTeacherSubscription { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = new();
    public List<PasswordResetToken> PasswordResetTokens { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
    public List<ActivityLog> ActivityLogs { get; set; } = new();
    public List<Streak> Streaks { get; set; } = new();
    public List<LessonCompletion> LessonCompletions { get; set; } = new();
    public List<ModuleCompletion> ModuleCompletions { get; set; } = new();
    public List<FlashCardReview> FlashCardReviews { get; set; } = new();
    public List<QuizAttempt> QuizAttempts { get; set; } = new();
    public List<EssaySubmission> EssaySubmissions { get; set; } = new();
    public List<PronunciationProgress> PronunciationProgresses { get; set; } = new();
    public List<Notification> Notifications { get; set; } = new();

    public List<ExternalLogin> ExternalLogins { get; set; } = new();

    // ===== Password Methods =====
    public void SetPassword(string password) =>
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

    public bool VerifyPassword(string password) =>
        PasswordHash != null && BCrypt.Net.BCrypt.Verify(password, PasswordHash);

    public bool HasLocalPassword() => !string.IsNullOrEmpty(PasswordHash);



    public bool HasExternalLogin(string provider) =>
        ExternalLogins.Any(el => el.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase));

    public ExternalLogin? GetExternalLogin(string provider) =>
        ExternalLogins.FirstOrDefault(el => el.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase));

    public bool IsExternalUserOnly() =>
        !HasLocalPassword() && ExternalLogins.Count != 0;

    public bool CanUnlinkProvider(string provider) =>
        HasLocalPassword() || ExternalLogins.Any(el => !el.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase));
}
