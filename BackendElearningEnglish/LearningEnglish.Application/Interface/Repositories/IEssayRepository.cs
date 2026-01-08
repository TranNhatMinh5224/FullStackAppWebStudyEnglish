using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IEssayRepository
    {
        // Tạo bài tự luận
        Task<Essay> CreateEssayAsync(Essay essay);
        
        // Lấy bài tự luận theo ID
        Task<Essay?> GetEssayByIdAsync(int essayId);
        
        // Lấy bài tự luận với chi tiết
        Task<Essay?> GetEssayByIdWithDetailsAsync(int essayId);
        
        // Lấy tất cả bài tự luận của assessment
        Task<List<Essay>> GetEssaysByAssessmentIdAsync(int assessmentId);
        
        // Cập nhật bài tự luận
        Task<Essay> UpdateEssayAsync(Essay essay);
        
        // Xóa bài tự luận
        Task DeleteEssayAsync(int essayId);

        // Kiểm tra assessment tồn tại
        Task<bool> AssessmentExistsAsync(int assessmentId);
        
        // Kiểm tra giáo viên sở hữu assessment
        Task<bool> IsTeacherOwnerOfAssessmentAsync(int teacherId, int assessmentId);
        
        // Kiểm tra bài tự luận tồn tại
        Task<bool> EssayExistsAsync(int essayId);
    }
}