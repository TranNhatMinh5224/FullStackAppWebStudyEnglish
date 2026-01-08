namespace LearningEnglish.Domain.Entities
{
    public class ExternalLogin
    {
        public int ExternalLoginId { get; set; }

        public string Provider { get; set; } = string.Empty;

        public string ProviderUserId { get; set; } = string.Empty;

        public string? ProviderDisplayName { get; set; }

        public string? ProviderPhotoUrl { get; set; }

        public string? ProviderEmail { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastUsedAt { get; set; }

        // Foreign Key
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
