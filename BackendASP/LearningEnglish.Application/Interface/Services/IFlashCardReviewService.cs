using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IFlashCardReviewService
    {
        // Đánh giá flashcard
        Task<ServiceResponse<ReviewFlashCardResponseDto>> ReviewFlashCardAsync(int userId, ReviewFlashCardDto reviewDto);

        // Lấy flashcard cần ôn hôm nay
        Task<ServiceResponse<DueFlashCardsResponseDto>> GetDueFlashCardsAsync(int userId);

        // Lấy thống kê ôn tập
        Task<ServiceResponse<ReviewStatisticsDto>> GetReviewStatisticsAsync(int userId);

        // Bắt đầu học module
        Task<ServiceResponse<int>> StartLearningModuleAsync(int userId, int moduleId);

        // Lấy flashcard đã thành thạo
        Task<ServiceResponse<DueFlashCardsResponseDto>> GetMasteredFlashCardsAsync(int userId);

        // Đếm số flashcard cần ôn
        Task<int> GetDueCountAsync(int userId);
    }
}
