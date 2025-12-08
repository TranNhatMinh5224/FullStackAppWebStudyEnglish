namespace LearningEnglish.Domain.Entities
{
    public class ExternalLogin
    {
        public int ExternalLoginId { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string ProviderUserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
              