using AutoMapper;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Mappings
{
    public class PronunciationAssessmentProfile : Profile
    {
        public PronunciationAssessmentProfile()
        {
            CreateMap<PronunciationAssessment, PronunciationAssessmentDto>()
                .ForMember(dest => dest.AudioUrl, opt => opt.MapFrom(src => src.AudioKey))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : null))
                .ForMember(dest => dest.FlashCardWord, opt => opt.MapFrom(src => src.FlashCard != null ? src.FlashCard.Word : null));

            CreateMap<PronunciationAssessment, ListPronunciationAssessmentDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.FlashCardWord, opt => opt.MapFrom(src => src.FlashCard != null ? src.FlashCard.Word : null));
        }
    }
}
