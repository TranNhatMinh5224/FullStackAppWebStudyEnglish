using AutoMapper;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Course mappings - Request DTOs to Entity
            CreateMap<AdminCreateCourseRequestDto, Course>()
                .ForMember(dest => dest.ImageKey, opt => opt.Ignore())
                .ForMember(dest => dest.ImageType, opt => opt.MapFrom(src => src.ImageType))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<TeacherCreateCourseRequestDto, Course>()
                .ForMember(dest => dest.ImageKey, opt => opt.Ignore())
                .ForMember(dest => dest.ImageType, opt => opt.MapFrom(src => src.ImageType))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Course mappings - Entity to Response DTOs
            CreateMap<Course, CourseResponseDto>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DescriptionMarkdown))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageKey))
                .ForMember(dest => dest.ImageType, opt => opt.MapFrom(src => src.ImageType))
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher != null ? $"{src.Teacher.FirstName} {src.Teacher.LastName}" : string.Empty))
                .ForMember(dest => dest.LessonCount, opt => opt.MapFrom(src => src.Lessons.Count))
                .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.UserCourses.Count));

            CreateMap<Course, AdminCourseListResponseDto>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DescriptionMarkdown))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageKey))
                .ForMember(dest => dest.ImageType, opt => opt.MapFrom(src => src.ImageType))
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher != null ? $"{src.Teacher.FirstName} {src.Teacher.LastName}" : string.Empty))
                .ForMember(dest => dest.LessonCount, opt => opt.MapFrom(src => src.Lessons.Count))
                .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.UserCourses.Count));

            // Mapping cho GET /api/user/courses/system-courses
            CreateMap<Course, SystemCoursesListResponseDto>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DescriptionMarkdown))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageKey))
                .ForMember(dest => dest.IsEnrolled, opt => opt.Ignore()); // Set trong service

            // Mapping cho GET /api/user/courses/{courseId}
            CreateMap<Course, CourseDetailWithEnrollmentDto>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DescriptionMarkdown))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageKey))
                .ForMember(dest => dest.TotalLessons, opt => opt.MapFrom(src => src.Lessons.Count))
                .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons))
                .ForMember(dest => dest.IsEnrolled, opt => opt.Ignore()); // Set trong service

            CreateMap<Lesson, LessonSummaryDto>()
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.OrderIndex));

            // Course with Progress mapping (for enrolled courses)
            CreateMap<Course, EnrolledCourseWithProgressDto>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DescriptionMarkdown))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageKey))
                .ForMember(dest => dest.ImageType, opt => opt.MapFrom(src => src.ImageType))
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher != null ? $"{src.Teacher.FirstName} {src.Teacher.LastName}" : string.Empty))
                .ForMember(dest => dest.LessonCount, opt => opt.MapFrom(src => src.Lessons.Count))
                .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.UserCourses.Count))
                .ForMember(dest => dest.ProgressPercentage, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.CompletedLessons, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.TotalLessons, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.IsCompleted, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.EnrolledAt, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore()); // Set in service

            // Course Update mappings
            CreateMap<AdminUpdateCourseRequestDto, Course>()
                .ForMember(dest => dest.ImageKey, opt => opt.Ignore())
                .ForMember(dest => dest.ImageType, opt => opt.MapFrom(src => src.ImageType))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<TeacherUpdateCourseRequestDto, Course>()
                .ForMember(dest => dest.ImageKey, opt => opt.Ignore())
                .ForMember(dest => dest.ImageType, opt => opt.MapFrom(src => src.ImageType))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Lesson mappings
            CreateMap<Lesson, LessonDto>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageKey));

            CreateMap<Lesson, LessonWithProgressDto>()
    .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageKey))
    .ForMember(dest => dest.CompletionPercentage, opt => opt.Ignore()) // Set in service
    .ForMember(dest => dest.IsCompleted, opt => opt.Ignore()) // Set in service
    .ForMember(dest => dest.CompletedModules, opt => opt.Ignore()) // Set in service
    .ForMember(dest => dest.TotalModules, opt => opt.Ignore()) // Set in service
    .ForMember(dest => dest.VideoProgressPercentage, opt => opt.Ignore()) // Set in service
    .ForMember(dest => dest.StartedAt, opt => opt.Ignore()) // Set in service
    .ForMember(dest => dest.CompletedAt, opt => opt.Ignore()); // Set in service

            CreateMap<AdminCreateLessonDto, Lesson>()
                .ForMember(dest => dest.LessonId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ImageKey, opt => opt.Ignore()); // Set manually in service after commit

            CreateMap<TeacherCreateLessonDto, Lesson>()
                .ForMember(dest => dest.LessonId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ImageKey, opt => opt.Ignore()); // Set manually in service after commit

            CreateMap<UpdateLessonDto, Lesson>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ImageKey, opt => opt.Ignore()) // Set manually in service after commit
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Module mappings
            CreateMap<Module, ModuleDto>()
                .ForMember(dest => dest.LessonTitle, opt => opt.MapFrom(src => src.Lesson != null ? src.Lesson.Title : string.Empty))
                .ForMember(dest => dest.LectureCount, opt => opt.MapFrom(src => src.Lectures.Count))
                .ForMember(dest => dest.FlashCardCount, opt => opt.MapFrom(src => src.FlashCards.Count))
                .ForMember(dest => dest.AssessmentCount, opt => opt.MapFrom(src => src.Assessments.Count));

            CreateMap<Module, ListModuleDto>();

            CreateMap<CreateModuleDto, Module>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateModuleDto, Module>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Module, ModuleWithProgressDto>()
                .ForMember(dest => dest.LessonTitle, opt => opt.MapFrom(src => src.Lesson != null ? src.Lesson.Title : string.Empty))
                .ForMember(dest => dest.LectureCount, opt => opt.MapFrom(src => src.Lectures.Count))
                .ForMember(dest => dest.FlashCardCount, opt => opt.MapFrom(src => src.FlashCards.Count))
                .ForMember(dest => dest.AssessmentCount, opt => opt.MapFrom(src => src.Assessments.Count))
                .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.ProgressPercentage, opt => opt.Ignore())
                .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore());

            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.AvatarUrl, opt => opt.Ignore()); // Handled in service layer

            CreateMap<RegisterUserDto, User>()
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

            CreateMap<UpdateUserDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateAvatarDto, User>()
                .ForMember(dest => dest.AvatarKey, opt => opt.Ignore()) // Set manually in service after commit
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // TeacherPackage mappings
            CreateMap<CreateTeacherPackageDto, TeacherPackage>();
            CreateMap<TeacherPackage, TeacherPackageDto>();

            // Lecture mappings
            CreateMap<Lecture, LectureDto>()
                .ForMember(dest => dest.MediaUrl, opt => opt.MapFrom(src => src.MediaKey))
                .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Module != null ? src.Module.Name : string.Empty))
                .ForMember(dest => dest.ParentTitle, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Title : string.Empty))
                .ForMember(dest => dest.ChildrenCount, opt => opt.MapFrom(src => src.Children.Count))
                .ForMember(dest => dest.AssessmentCount, opt => opt.Ignore()); // Assessment không còn thuộc về Lecture

            CreateMap<Lecture, ListLectureDto>()
                .ForMember(dest => dest.MediaUrl, opt => opt.MapFrom(src => src.MediaKey))
                .ForMember(dest => dest.ChildrenCount, opt => opt.MapFrom(src => src.Children.Count));

            CreateMap<Lecture, LectureTreeDto>()
                .ForMember(dest => dest.ChildrenCount, opt => opt.MapFrom(src => src.Children.Count))
                .ForMember(dest => dest.Children, opt => opt.Ignore()); // Sẽ build trong service

            CreateMap<CreateLectureDto, Lecture>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.MediaKey, opt => opt.Ignore()); // Set manually in service after commit

            CreateMap<UpdateLectureDto, Lecture>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.MediaKey, opt => opt.Ignore()) // Set manually in service after commit
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Lecture, LectureWithProgressDto>()
                .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Module != null ? src.Module.Name : string.Empty))
                .ForMember(dest => dest.ParentTitle, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Title : string.Empty))
                .ForMember(dest => dest.ChildrenCount, opt => opt.MapFrom(src => src.Children.Count))
                .ForMember(dest => dest.AssessmentCount, opt => opt.Ignore()) // Assessment không còn thuộc về Lecture
                .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.ProgressPercentage, opt => opt.Ignore())
                .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TimeSpentSeconds, opt => opt.Ignore());

            // TeacherSubscription mappings
            CreateMap<TeacherSubscription, ResPurchaseTeacherPackageDto>()
            .ForMember(dest => dest.IdTeacherPackage, opt => opt.MapFrom(src => src.TeacherSubscriptionId))
            .ForMember(dest => dest.IdUser, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
            .ForMember(dest => dest.PackageName, opt => opt.MapFrom(src => src.TeacherPackage != null ? src.TeacherPackage.PackageName : string.Empty))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.TeacherPackage != null ? src.TeacherPackage.Price : 0))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));

            // Mapping for UserDto TeacherSubscription property
            CreateMap<TeacherSubscription, UserTeacherSubscriptionDto>()
            .ForMember(dest => dest.IsTeacher, opt => opt.MapFrom(src => src.Status == SubscriptionStatus.Active))
            .ForMember(dest => dest.PackageLevel, opt => opt.MapFrom(src =>
                src.Status == SubscriptionStatus.Active && src.TeacherPackage != null
                ? src.TeacherPackage.Level.ToString()
                : null));

            // FlashCard mappings
            CreateMap<FlashCard, FlashCardDto>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageKey)) // Map key to url field
                .ForMember(dest => dest.AudioUrl, opt => opt.MapFrom(src => src.AudioKey)) // Map key to url field
                .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Module != null ? src.Module.Name : string.Empty))
                .ForMember(dest => dest.ReviewCount, opt => opt.Ignore()) // Sẽ tính trong service
                .ForMember(dest => dest.SuccessRate, opt => opt.Ignore()) // Sẽ tính trong service
                .ForMember(dest => dest.CurrentLevel, opt => opt.Ignore()) // Sẽ tính trong service
                .ForMember(dest => dest.NextReviewAt, opt => opt.Ignore()) // Sẽ tính trong service
                .ForMember(dest => dest.LastReviewedAt, opt => opt.Ignore()); // Sẽ tính trong service

            CreateMap<FlashCard, ListFlashCardDto>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageKey)) // Map key to url field
                .ForMember(dest => dest.AudioUrl, opt => opt.MapFrom(src => src.AudioKey)) // Map key to url field
                .ForMember(dest => dest.ReviewCount, opt => opt.Ignore()) // Sẽ tính trong service
                .ForMember(dest => dest.SuccessRate, opt => opt.Ignore()) // Sẽ tính trong service
                .ForMember(dest => dest.CurrentLevel, opt => opt.Ignore()); // Sẽ tính trong service

            CreateMap<CreateFlashCardDto, FlashCard>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ImageKey, opt => opt.Ignore()) // Set manually in service after commit
                .ForMember(dest => dest.AudioKey, opt => opt.Ignore()); // Set manually in service after commit

            CreateMap<UpdateFlashCardDto, FlashCard>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ImageKey, opt => opt.Ignore()) // Set manually in service after commit
                .ForMember(dest => dest.AudioKey, opt => opt.Ignore()) // Set manually in service after commit
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Assessment mappings
            CreateMap<Assessment, AssessmentDto>()
                .ForMember(dest => dest.ModuleTitle, opt => opt.MapFrom(src => src.Module != null ? src.Module.Name : string.Empty))
                .ForMember(dest => dest.TimeLimit, opt => opt.MapFrom(src => src.TimeLimit.HasValue ? src.TimeLimit.Value.ToString(@"hh\:mm\:ss") : null));

            CreateMap<CreateAssessmentDto, Assessment>()
                .ForMember(dest => dest.AssessmentId, opt => opt.Ignore())
                .ForMember(dest => dest.TimeLimit, opt => opt.MapFrom(src => ParseTimeSpan(src.TimeLimit)));

            // Essay mappings
            CreateMap<Essay, EssayDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

            CreateMap<CreateEssayDto, Essay>()
                .ForMember(dest => dest.EssayId, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Domain.Enums.AssessmentType.Essay));

            // EssaySubmission mappings
            CreateMap<EssaySubmission, EssaySubmissionDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.AttachmentUrl, opt => opt.MapFrom(src => src.AttachmentKey)); // Will be replaced with URL in service

            // EssaySubmission to List DTO (basic info for listing)
            CreateMap<EssaySubmission, EssaySubmissionListDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.UserAvatarUrl, opt => opt.Ignore()) // Will be built in service
                .ForMember(dest => dest.HasAttachment, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.AttachmentKey)));

            CreateMap<CreateEssaySubmissionDto, EssaySubmission>()
                .ForMember(dest => dest.SubmissionId, opt => opt.Ignore())
                .ForMember(dest => dest.SubmittedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Enums.SubmissionStatus.Submitted))
                .ForMember(dest => dest.AttachmentKey, opt => opt.Ignore()); // Set manually in service after MinIO commit

            CreateMap<UpdateEssaySubmissionDto, EssaySubmission>()
                .ForMember(dest => dest.AttachmentKey, opt => opt.Ignore()) // Set manually in service after MinIO commit
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Quiz mappings
            CreateMap<Quiz, QuizDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            CreateMap<QuizCreateDto, Quiz>()
                .ForMember(dest => dest.QuizId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<QuizUpdateDto, Quiz>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // QuizSection mappings
            CreateMap<QuizSection, QuizSectionDto>();
            CreateMap<QuizSection, ListQuizSectionDto>();

            CreateMap<CreateQuizSectionDto, QuizSection>()
                .ForMember(dest => dest.QuizSectionId, opt => opt.Ignore());

            CreateMap<UpdateQuizSectionDto, QuizSection>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // QuizGroup mappings
            CreateMap<QuizGroup, QuizGroupDto>()
                .ForMember(dest => dest.ImgUrl, opt => opt.MapFrom(src => src.ImgKey))
                .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom(src => src.VideoKey));

            CreateMap<CreateQuizGroupDto, QuizGroup>()
                .ForMember(dest => dest.QuizGroupId, opt => opt.Ignore())
                .ForMember(dest => dest.ImgKey, opt => opt.Ignore()) // Set manually in service after commit
                .ForMember(dest => dest.VideoKey, opt => opt.Ignore()); // Set manually in service after commit

            CreateMap<UpdateQuizGroupDto, QuizGroup>()
                .ForMember(dest => dest.ImgKey, opt => opt.Ignore()) // Set manually in service after commit
                .ForMember(dest => dest.VideoKey, opt => opt.Ignore()) // Set manually in service after commit
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Question mappings
            CreateMap<Question, QuestionReadDto>()
                .ForMember(dest => dest.MediaUrl, opt => opt.MapFrom(src => src.MediaKey))
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

            CreateMap<QuestionCreateDto, Question>()
                .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.MediaKey, opt => opt.Ignore()) // Set manually in service after commit
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

            CreateMap<QuestionUpdateDto, Question>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.MediaKey, opt => opt.Ignore()) // Set manually in service after commit
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // AnswerOption mappings
            CreateMap<AnswerOption, AnswerOptionReadDto>()
                .ForMember(dest => dest.MediaUrl, opt => opt.MapFrom(src => src.MediaKey));

            CreateMap<AnswerOptionCreateDto, AnswerOption>()
                .ForMember(dest => dest.AnswerOptionId, opt => opt.Ignore())
                .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
                .ForMember(dest => dest.MediaKey, opt => opt.Ignore()); // 

            // QuizAttempt mappings
            CreateMap<QuizAttempt, QuizAttemptDto>();

            CreateMap<QuizAttempt, QuizAttemptWithQuestionsDto>()
                .ForMember(dest => dest.QuizSections, opt => opt.Ignore());

            CreateMap<QuizAttempt, QuizAttemptResultDto>();



        }


        private static TimeSpan? ParseTimeSpan(string? timeLimitString)
        {
            if (string.IsNullOrEmpty(timeLimitString))
                return null;

            if (TimeSpan.TryParse(timeLimitString, out var timespan))
                return timespan;

            return null;
        }
    }
}
