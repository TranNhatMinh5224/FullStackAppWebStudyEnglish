using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IFlashCardReviewService
    {
        /// <summary>
        /// Submit a flashcard review with quality rating (0-5)
        /// Uses Spaced Repetition Algorithm (SM-2) to calculate next review date
        /// </summary>
        Task<ServiceResponse<ReviewFlashCardResponseDto>> ReviewFlashCardAsync(int userId, ReviewFlashCardDto reviewDto);

        /// <summary>
        /// Get all flashcards due for review today (NextReviewDate <= Today)
        /// Includes both new cards and cards being reviewed
        /// </summary>
        Task<ServiceResponse<DueFlashCardsResponseDto>> GetDueFlashCardsAsync(int userId);

        /// <summary>
        /// Get flashcards due for review by module
        /// </summary>
        Task<ServiceResponse<DueFlashCardsResponseDto>> GetDueFlashCardsByModuleAsync(int userId, int moduleId);

        /// <summary>
        /// Get comprehensive review statistics for dashboard
        /// </summary>
        Task<ServiceResponse<ReviewStatisticsDto>> GetReviewStatisticsAsync(int userId);

        /// <summary>
        /// Get count of flashcards due today
        /// </summary>
        Task<ServiceResponse<int>> GetDueCountAsync(int userId);

        /// <summary>
        /// Start learning a module - Add all flashcards in module to review system
        /// Creates initial FlashCardReview records for all cards in the module
        /// </summary>
        Task<ServiceResponse<int>> StartLearningModuleAsync(int userId, int moduleId);

        /// <summary>
        /// Get all mastered flashcards (cards that won't be reviewed anymore)
        /// </summary>
        Task<ServiceResponse<DueFlashCardsResponseDto>> GetMasteredFlashCardsAsync(int userId);
    }
}
