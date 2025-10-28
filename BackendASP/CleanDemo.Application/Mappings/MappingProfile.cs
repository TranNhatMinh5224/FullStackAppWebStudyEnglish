using AutoMapper;
using CleanDemo.Domain.Entities;
using CleanDemo.Application.DTOs;

namespace CleanDemo.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AdminCreateCourseRequestDto, Course>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<TeacherCreateCourseRequestDto, Course>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<AdminUpdateCourseRequestDto, Course>()
                .ForMember(dest => dest.CourseId, opt => opt.Ignore())
                .ForMember(dest => dest.TeacherId, opt => opt.Ignore())
                .ForMember(dest => dest.ClassCode, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.EnrollmentCount, opt => opt.Ignore())
                .ForMember(dest => dest.Teacher, opt => opt.Ignore())
                .ForMember(dest => dest.Lessons, opt => opt.Ignore())
                .ForMember(dest => dest.UserCourses, opt => opt.Ignore());

            CreateMap<TeacherUpdateCourseRequestDto, Course>()
                .ForMember(dest => dest.CourseId, opt => opt.Ignore())
                .ForMember(dest => dest.TeacherId, opt => opt.Ignore())
                .ForMember(dest => dest.Price, opt => opt.Ignore())
                .ForMember(dest => dest.ClassCode, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.EnrollmentCount, opt => opt.Ignore())
                .ForMember(dest => dest.IsFeatured, opt => opt.Ignore())
                .ForMember(dest => dest.Teacher, opt => opt.Ignore())
                .ForMember(dest => dest.Lessons, opt => opt.Ignore())
                .ForMember(dest => dest.UserCourses, opt => opt.Ignore());

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
                .ForMember(dest => dest.IsEnrolled, opt => opt.Ignore());

            CreateMap<Course, CourseDetailResponseDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher != null ? $"{src.Teacher.FirstName} {src.Teacher.LastName}" : string.Empty))
                .ForMember(dest => dest.LessonCount, opt => opt.MapFrom(src => src.Lessons.Count))
                .ForMember(dest => dest.IsEnrolled, opt => opt.Ignore())
                .ForMember(dest => dest.Lessons, opt => opt.Ignore());

            CreateMap<Lesson, LessonDto>();
            CreateMap<Lesson, ListLessonDto>();

            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

            CreateMap<RegisterUserDto, User>()
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

            CreateMap<UpdateUserDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CreateTeacherPackageDto, TeacherPackage>();
            CreateMap<UpdateTeacherPackageDto, TeacherPackage>();
            CreateMap<TeacherPackage, TeacherPackageDto>();

            CreateMap<TeacherSubscription, ResPurchaseTeacherPackageDto>()
            .ForMember(dest => dest.IdTeacherPackage, opt => opt.MapFrom(src => src.TeacherSubscriptionId))
            .ForMember(dest => dest.IdUser, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
            .ForMember(dest => dest.PackageName, opt => opt.MapFrom(src => src.TeacherPackage != null ? src.TeacherPackage.PackageName : string.Empty))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.TeacherPackage != null ? src.TeacherPackage.Price : 0))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));
           
            
            
        }
    }
}
