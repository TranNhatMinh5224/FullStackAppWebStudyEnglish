namespace LearningEnglish.Application.DTOs
{
    public class ScoringResultDto
    {
        public bool IsCorrect { get; set; }
        public decimal PointsEarned { get; set; }
        public decimal Percentage { get; set; }
        public string? Feedback { get; set; }
        public List<int>? CorrectAnswerIds { get; set; }
    }

    public class QuizAttemptScoringResultDto
    {
        public int AttemptId { get; set; }
        public decimal TotalPointsEarned { get; set; }
        public decimal TotalMaxPoints { get; set; }
        public decimal Percentage { get; set; }
        public List<QuestionScoringDto> QuestionResults { get; set; } = new();
    }

    public class QuestionScoringDto
    {
        public int QuestionId { get; set; }
        public bool IsCorrect { get; set; }
        public decimal PointsEarned { get; set; }
        public decimal MaxPoints { get; set; }
        public decimal Percentage { get; set; }
        public string? Feedback { get; set; }
        public List<int>? CorrectAnswerIds { get; set; }
    }
}