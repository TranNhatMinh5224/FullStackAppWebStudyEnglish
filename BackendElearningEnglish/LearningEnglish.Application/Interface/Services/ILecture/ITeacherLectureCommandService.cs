using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface.Services.Lecture
{
    public interface ITeacherLectureCommandService
    {
        // Teacher tạo lecture
        Task<ServiceResponse<LectureDto>> TeacherCreateLecture(CreateLectureDto createLectureDto, int teacherId);

        // Teacher tạo nhiều lecture
        Task<ServiceResponse<BulkCreateLecturesResponseDto>> TeacherBulkCreateLectures(BulkCreateLecturesDto bulkCreateDto, int teacherId);

        // Teacher cập nhật lecture
        Task<ServiceResponse<LectureDto>> UpdateLecture(int lectureId, UpdateLectureDto updateLectureDto, int teacherId);

        // Teacher xóa lecture
        Task<ServiceResponse<bool>> DeleteLecture(int lectureId, int teacherId);

        // Teacher sắp xếp lại lecture
        Task<ServiceResponse<bool>> ReorderLectures(List<ReorderLectureDto> reorderDtos, int teacherId);
    }
}