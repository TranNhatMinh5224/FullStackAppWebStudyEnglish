namespace LearningEnglish.Application.DTOs
{
    public class QuizScoreDto
    {
        public int AttemptId { get; set; }
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int AttemptNumber { get; set; }
        public decimal TotalScore { get; set; }
        public decimal Percentage { get; set; }
        public bool IsPassed { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int TimeSpentSeconds { get; set; }
    }
}
