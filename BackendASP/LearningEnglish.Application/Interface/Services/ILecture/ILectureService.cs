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
        
        // Admin tạo lecture
        Task<ServiceResponse<LectureDto>> AdminCreateLecture(CreateLectureDto createLectureDto);
        
        // Teacher tạo lecture
        Task<ServiceResponse<LectureDto>> TeacherCreateLecture(CreateLectureDto createLectureDto, int teacherId);
        
        // Admin tạo nhiều lecture
        Task<ServiceResponse<BulkCreateLecturesResponseDto>> AdminBulkCreateLectures(BulkCreateLecturesDto bulkCreateDto);
        
        // Teacher tạo nhiều lecture
        Task<ServiceResponse<BulkCreateLecturesResponseDto>> TeacherBulkCreateLectures(BulkCreateLecturesDto bulkCreateDto, int teacherId);
        
        // Cập nhật lecture (RLS đã filter theo ownership)
        Task<ServiceResponse<LectureDto>> UpdateLecture(int lectureId, UpdateLectureDto updateLectureDto);
        
        // Xóa lecture (RLS đã filter theo ownership)
        Task<ServiceResponse<bool>> DeleteLecture(int lectureId);
        
        // Sắp xếp lại lecture (RLS đã filter theo ownership)
        Task<ServiceResponse<bool>> ReorderLectures(List<ReorderLectureDto> reorderDtos);
    }
}
