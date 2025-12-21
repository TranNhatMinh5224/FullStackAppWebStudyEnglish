using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{

    public interface IPronunciationAssessmentService
    {
        // Tạo đánh giá phát âm theo thời gian thực (không lưu trữ, chỉ cập nhật tiến độ)
        Task<ServiceResponse<PronunciationAssessmentDto>> CreateAssessmentAsync(
            CreatePronunciationAssessmentDto dto,
            int userId);

        // Lấy danh sách flashcard kèm tiến độ phát âm theo module
        Task<ServiceResponse<List<FlashCardWithPronunciationDto>>> GetFlashCardsWithPronunciationProgressAsync(
            int moduleId,
            int userId);

        // Lấy tổng hợp kết quả/thống kê phát âm cho module
        Task<ServiceResponse<ModulePronunciationSummaryDto>> GetModulePronunciationSummaryAsync(
            int moduleId,
            int userId);
    }
}