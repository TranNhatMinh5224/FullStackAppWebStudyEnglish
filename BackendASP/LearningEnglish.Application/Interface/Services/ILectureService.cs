using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface ILectureService
    {
        // Lấy thông tin lecture
        Task<ServiceResponse<LectureDto>> GetLectureByIdAsync(int lectureId, int? userId = null);
        
        // Lấy danh sách lecture theo module
        Task<ServiceResponse<List<ListLectureDto>>> GetLecturesByModuleIdAsync(int moduleId, int? userId = null);
        
        // Lấy cây lecture
        Task<ServiceResponse<List<LectureTreeDto>>> GetLectureTreeByModuleIdAsync(int moduleId, int? userId = null);
        
        // Tạo lecture
        Task<ServiceResponse<LectureDto>> CreateLectureAsync(CreateLectureDto createLectureDto, int createdByUserId);
        
        // Tạo nhiều lecture
        Task<ServiceResponse<BulkCreateLecturesResponseDto>> BulkCreateLecturesAsync(BulkCreateLecturesDto bulkCreateDto, int createdByUserId);
        
        // Cập nhật lecture
        Task<ServiceResponse<LectureDto>> UpdateLectureAsync(int lectureId, UpdateLectureDto updateLectureDto, int updatedByUserId);
        
        // Xóa lecture
        Task<ServiceResponse<bool>> DeleteLectureAsync(int lectureId, int deletedByUserId);

        // Cập nhật lecture có kiểm tra quyền
        Task<ServiceResponse<LectureDto>> UpdateLectureWithAuthorizationAsync(int lectureId, UpdateLectureDto updateLectureDto, int userId, string userRole);
        
        // Xóa lecture có kiểm tra quyền
        Task<ServiceResponse<bool>> DeleteLectureWithAuthorizationAsync(int lectureId, int userId, string userRole);

        // Kiểm tra quyền teacher
        Task<bool> CheckTeacherLecturePermission(int lectureId, int teacherId);
        
        // Sắp xếp lại lecture
        Task<ServiceResponse<bool>> ReorderLecturesAsync(List<ReorderLectureDto> reorderDtos, int userId, string userRole);
    }
}
