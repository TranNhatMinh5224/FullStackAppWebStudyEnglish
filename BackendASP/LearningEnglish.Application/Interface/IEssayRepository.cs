using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IEssayRepository
    {
        // CRUD cho Essay(Bài kiểm tra tự luận)
        Task<Essay> CreateEssayAsync(Essay essay);
        Task<Essay?> GetEssayByIdAsync(int essayId);
        Task<Essay?> GetEssayByIdWithDetailsAsync(int essayId);
        Task<List<Essay>> GetEssaysByAssessmentIdAsync(int assessmentId);
        Task<Essay> UpdateEssayAsync(Essay essay);
        Task DeleteEssayAsync(int essayId);
        
        // Kiểm tra tồn tại và quyền hạn
        Task<bool> AssessmentExistsAsync(int assessmentId);
        Task<bool> IsTeacherOwnerOfAssessmentAsync(int teacherId, int assessmentId);
        Task<bool> EssayExistsAsync(int essayId);
    }
}