using AutoMapper;
using CleanDemo.Domain.Entities;
using CleanDemo.Application.DTOs;

namespace CleanDemo.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Course mappings - Request DTOs to Entity
            CreateMap<AdminCreateCourseRequestDto, Course>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<TeacherCreateCourseRequestDto, Course>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Course mappings - Entity to Response DTOs
            CreateMap<Course, CourseResponseDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher != null ? $"{src.Teacher.FirstName} {src.Teacher.LastName}" : string.Empty))
                .ForMember(dest => dest.LessonCount, opt => opt.MapFrom(src => src.Lessons.Count))
                .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.UserCourses.Count));

            CreateMap<Course, AdminCourseListResponseDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher != null ? $"{src.Teacher.FirstName} {src.Teacher.LastName}" : string.Empty))
                .ForMember(dest => dest.LessonCount, opt => opt.MapFrom(src => src.Lessons.Count))
                .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.UserCourses.Count));

            CreateMap<Course, UserCourseListResponseDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher != null ? $"{src.Teacher.FirstName} {src.Teacher.LastName}" : string.Empty))
                .ForMember(dest => dest.IsEnrolled, opt => opt.Ignore()); // Sẽ set trong service

            CreateMap<Course, CourseDetailResponseDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher != null ? $"{src.Teacher.FirstName} {src.Teacher.LastName}" : string.Empty))
                .ForMember(dest => dest.LessonCount, opt => opt.MapFrom(src => src.Lessons.Count))
                .ForMember(dest => dest.IsEnrolled, opt => opt.Ignore()) 
                .ForMember(dest => dest.Lessons, opt => opt.Ignore()); 

            // Lesson mappings
            CreateMap<Lesson, LessonDto>();

            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));  // Sửa SureName thành LastName

            CreateMap<RegisterUserDto, User>()
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));  // Sửa SureName thành LastName, và dest.FirstName thành dest.LastName nếu cần

            CreateMap<UpdateUserDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // TeacherPackage mappings
            CreateMap<CreateTeacherPackageDto, TeacherPackage>();
            CreateMap<UpdateTeacherPackageDto, TeacherPackage>();
            CreateMap<TeacherPackage, TeacherPackageDto>();
        }
    }
}
