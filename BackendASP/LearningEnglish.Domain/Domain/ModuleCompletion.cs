namespace LearningEnglish.Domain.Entities
{
    public class ModuleCompletion
    {
        public int ModuleCompletionId { get; set; }
        public int ModuleId { get; set; }
        public int UserId { get; set; }
        
        // ===== EXISTING =====
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
        
        // ===== NEW SIMPLE ADDITIONS =====
        public bool IsCompleted { get; set; } = true; // Default true when record created
        public decimal ProgressPercentage { get; set; } = 100; // Always 100% when completed
        public DateTime? StartedAt { get; set; }

        // Navigation Properties
        public Module? Module { get; set; }
        public User? User { get; set; }
        
        // ===== BUSINESS LOGIC =====
        public void MarkAsCompleted()
        {
            IsCompleted = true;
            ProgressPercentage = 100;
            CompletedAt = DateTime.UtcNow;
            if (StartedAt == null) StartedAt = DateTime.UtcNow;
        }
        
        public void MarkAsStarted()
        {
            if (StartedAt == null)
            {
                StartedAt = DateTime.UtcNow;
                IsCompleted = false;
                ProgressPercentage = 0;
            }
        }
    }
}
