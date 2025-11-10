namespace CleanDemo.Domain.Entities
{
    public class ModuleCompletion
    {
        public int ModuleCompletionId { get; set; }
        public int ModuleId { get; set; }
        public int UserId { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Module? Module { get; set; }
        public User? User { get; set; }
    }
}