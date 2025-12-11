using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface ILectureService
    {
        // Basic CRUD operations
        Task<ServiceResponse<LectureDto>> GetLectureByIdAsync(int lectureId, int? userId = null);
        Task<ServiceResponse<List<ListLectureDto>>> GetLecturesByModuleIdAsync(int moduleId, int? userId = null);
        Task<ServiceResponse<List<LectureTreeDto>>> GetLectureTreeByModuleIdAsync(int moduleId, int? userId = null);
        Task<ServiceResponse<LectureDto>> CreateLectureAsync(CreateLectureDto createLectureDto, int createdByUserId);
        Task<ServiceResponse<BulkCreateLecturesResponseDto>> BulkCreateLecturesAsync(BulkCreateLecturesDto bulkCreateDto, int createdByUserId);
        Task<ServiceResponse<LectureDto>> UpdateLectureAsync(int lectureId, UpdateLectureDto updateLectureDto, int updatedByUserId);
        Task<ServiceResponse<bool>> DeleteLectureAsync(int lectureId, int deletedByUserId);

        // Authorization methods
        Task<ServiceResponse<LectureDto>> UpdateLectureWithAuthorizationAsync(int lectureId, UpdateLectureDto updateLectureDto, int userId, string userRole);
        Task<ServiceResponse<bool>> DeleteLectureWithAuthorizationAsync(int lectureId, int userId, string userRole);

        // Helper methods
        Task<bool> CheckTeacherLecturePermission(int lectureId, int teacherId);
        Task<ServiceResponse<bool>> ReorderLecturesAsync(List<ReorderLectureDto> reorderDtos, int userId, string userRole);
    }
}
