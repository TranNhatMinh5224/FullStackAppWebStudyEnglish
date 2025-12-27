using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface.Services.Lecture
{
    public interface ITeacherLectureService
    {
        // Teacher tạo lecture
        Task<ServiceResponse<LectureDto>> TeacherCreateLecture(CreateLectureDto createLectureDto, int teacherId);
        
        // Teacher tạo nhiều lecture
        Task<ServiceResponse<BulkCreateLecturesResponseDto>> TeacherBulkCreateLectures(BulkCreateLecturesDto bulkCreateDto, int teacherId);
        
        // Teacher cập nhật lecture 
        Task<ServiceResponse<LectureDto>> UpdateLecture(int lectureId, UpdateLectureDto updateLectureDto);
        
        // Teacher xóa lecture 
        Task<ServiceResponse<bool>> DeleteLecture(int lectureId);
        
        // Teacher sắp xếp lại lecture 
        Task<ServiceResponse<bool>> ReorderLectures(List<ReorderLectureDto> reorderDtos);
        
        // Teacher lấy lecture theo ID 
        Task<ServiceResponse<LectureDto>> GetLectureByIdAsync(int lectureId);
        
        // Teacher lấy danh sách lecture theo module 
        Task<ServiceResponse<List<ListLectureDto>>> GetLecturesByModuleIdAsync(int moduleId);
        
        // Teacher lấy cây lecture 
        Task<ServiceResponse<List<LectureTreeDto>>> GetLectureTreeByModuleIdAsync(int moduleId);
    }
}
