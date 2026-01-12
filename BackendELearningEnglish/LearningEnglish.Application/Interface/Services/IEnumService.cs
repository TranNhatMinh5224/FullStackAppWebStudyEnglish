using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOS.Common;

namespace LearningEnglish.Application.Interface.Services
{
    public interface IEnumService
    {
        // Course Related
        ServiceResponse<List<EnumMappingDto>> GetCourseStatuses();
        ServiceResponse<List<EnumMappingDto>> GetCourseTypes();
        ServiceResponse<List<EnumMappingDto>> GetDifficultyLevels();

        // Quiz & Assessment Related
        ServiceResponse<List<EnumMappingDto>> GetQuizStatuses();
        ServiceResponse<List<EnumMappingDto>> GetQuizTypes();
        ServiceResponse<List<EnumMappingDto>> GetQuestionTypes();
        ServiceResponse<List<EnumMappingDto>> GetSubmissionStatuses();

        // Payment Related
        ServiceResponse<List<EnumMappingDto>> GetPaymentStatuses();
        ServiceResponse<List<EnumMappingDto>> GetProductTypes();

        // Asset Frontend Related
        ServiceResponse<List<EnumMappingDto>> GetAssetTypes();

        // Lecture Related
        ServiceResponse<List<EnumMappingDto>> GetLectureTypes();

        // Master Data (Gộp tất cả cho Frontend)
        ServiceResponse<Dictionary<string, List<EnumMappingDto>>> GetAllEnums();
    }
}
