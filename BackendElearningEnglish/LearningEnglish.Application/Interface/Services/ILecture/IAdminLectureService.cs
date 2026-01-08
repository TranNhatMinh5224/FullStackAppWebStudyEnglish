using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface.Services.Lecture
{
    public interface IAdminLectureService
    {
        // Admin tạo lecture
        Task<ServiceResponse<LectureDto>> AdminCreateLecture(CreateLectureDto createLectureDto);
        
        // Admin tạo nhiều lecture
        Task<ServiceResponse<BulkCreateLecturesResponseDto>> AdminBulkCreateLectures(BulkCreateLecturesDto bulkCreateDto);
        
        // Admin cập nhật lecture
        Task<ServiceResponse<LectureDto>> UpdateLecture(int lectureId, UpdateLectureDto updateLectureDto);
        
        // Admin xóa lecture
        Task<ServiceResponse<bool>> DeleteLecture(int lectureId);
        
        // Admin sắp xếp lại lecture
        Task<ServiceResponse<bool>> ReorderLectures(List<ReorderLectureDto> reorderDtos);
        
        // Admin lấy lecture theo ID 
        Task<ServiceResponse<LectureDto>> GetLectureByIdAsync(int lectureId);
        
        // Admin lấy danh sách lecture theo module 
        Task<ServiceResponse<List<ListLectureDto>>> GetLecturesByModuleIdAsync(int moduleId);
        
        // Admin lấy cây lecture 
        Task<ServiceResponse<List<LectureTreeDto>>> GetLectureTreeByModuleIdAsync(int moduleId);
    }
}
