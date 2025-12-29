namespace LearningEnglish.Application.Common.Pagination;

// Query parameters for QuizAttempt entity with search and pagination support
public class QuizAttemptQueryParameters : PageRequest
{
    // Search term for filtering quiz attempts (applied to User Email, Quiz Title, etc.)
    public string? SearchTerm { get; set; }
}
