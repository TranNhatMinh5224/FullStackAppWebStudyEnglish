using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOS.Common;
using LearningEnglish.Application.Interface.Services;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Service.EnumService
{
    public class EnumService : IEnumService
    {
        private readonly IMapper _mapper;

        public EnumService(IMapper mapper)
        {
            _mapper = mapper;
        }

        private ServiceResponse<List<EnumMappingDto>> GetEnums<TEnum>() where TEnum : struct, Enum
        {
            var values = Enum.GetValues<TEnum>().ToList();
            var dtos = _mapper.Map<List<EnumMappingDto>>(values);
            
            return new ServiceResponse<List<EnumMappingDto>>
            {
                Data = dtos,
                Success = true
            };
        }

        public ServiceResponse<List<EnumMappingDto>> GetCourseStatuses() => GetEnums<CourseStatus>();
        public ServiceResponse<List<EnumMappingDto>> GetCourseTypes() => GetEnums<CourseType>();
        public ServiceResponse<List<EnumMappingDto>> GetDifficultyLevels() => GetEnums<DifficultyLevel>();
        
        public ServiceResponse<List<EnumMappingDto>> GetQuizStatuses() => GetEnums<QuizStatus>();
        public ServiceResponse<List<EnumMappingDto>> GetQuizTypes() => GetEnums<QuizType>();
        public ServiceResponse<List<EnumMappingDto>> GetQuestionTypes() => GetEnums<QuestionType>();
        public ServiceResponse<List<EnumMappingDto>> GetSubmissionStatuses() => GetEnums<SubmissionStatus>();
        
        public ServiceResponse<List<EnumMappingDto>> GetPaymentStatuses() => GetEnums<PaymentStatus>();
        public ServiceResponse<List<EnumMappingDto>> GetProductTypes() => GetEnums<ProductType>();
        public ServiceResponse<List<EnumMappingDto>> GetAssetTypes() => GetEnums<AssetType>();
        public ServiceResponse<List<EnumMappingDto>> GetLectureTypes() => GetEnums<LectureType>();

        public ServiceResponse<Dictionary<string, List<EnumMappingDto>>> GetAllEnums()
        {
            var masterData = new Dictionary<string, List<EnumMappingDto>>
            {
                { "CourseStatus", GetCourseStatuses().Data ?? new() },
                { "CourseType", GetCourseTypes().Data ?? new() },
                { "DifficultyLevel", GetDifficultyLevels().Data ?? new() },
                { "QuizStatus", GetQuizStatuses().Data ?? new() },
                { "QuizType", GetQuizTypes().Data ?? new() },
                { "QuestionType", GetQuestionTypes().Data ?? new() },
                { "SubmissionStatus", GetSubmissionStatuses().Data ?? new() },
                { "PaymentStatus", GetPaymentStatuses().Data ?? new() },
                { "ProductType", GetProductTypes().Data ?? new() },
                { "AssetType", GetAssetTypes().Data ?? new() },
                { "LectureType", GetLectureTypes().Data ?? new() }
            };

            return new ServiceResponse<Dictionary<string, List<EnumMappingDto>>>
            {
                Data = masterData,
                Success = true
            };
        }
    }
}
