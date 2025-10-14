using AutoMapper;
using CleanDemo.Domain.Entities;
using CleanDemo.Application.DTOs;

namespace CleanDemo.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Course mappings
            CreateMap<Course, CourseDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.Ignore()) // Sẽ set trong service
                .ForMember(dest => dest.LessonCount, opt => opt.Ignore()) // Sẽ set trong service
                .ForMember(dest => dest.StudentCount, opt => opt.Ignore()); // Sẽ set trong service

            CreateMap<CreateCourseDto, Course>();

            CreateMap<Course, UserCourseDto>()
                .ForMember(dest => dest.IsEnrolled, opt => opt.Ignore()); // Sẽ set trong service

            CreateMap<Course, ListMyCourseTeacherDto>()
                .ForMember(dest => dest.StudentCount, opt => opt.Ignore()); // Sẽ set trong service

            CreateMap<Course, ListMyCourseStudentDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.Ignore()); // Sẽ set trong service

            CreateMap<Course, CourseDetailDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.Ignore()) // Sẽ set trong service
                .ForMember(dest => dest.LessonCount, opt => opt.Ignore()) // Sẽ set trong service
                .ForMember(dest => dest.IsEnrolled, opt => opt.Ignore()) // Sẽ set trong service
                .ForMember(dest => dest.Lessons, opt => opt.Ignore()); // Sẽ set trong service

            // Lesson mappings
            CreateMap<Lesson, LessonDto>();

            // User mappings (nếu chưa có)
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<RegisterUserDto, User>();
            CreateMap<UpdateUserDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
