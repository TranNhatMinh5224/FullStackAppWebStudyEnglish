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
            // Essay Grading mappings
            CreateMap<EssaySubmission, EssayGradingResultDto>()
                .ForMember(dest => dest.SubmissionId, opt => opt.MapFrom(src => src.SubmissionId))
                .ForMember(dest => dest.Score, opt => opt.MapFrom(src => src.TeacherScore ?? src.Score ?? 0))
                .ForMember(dest => dest.MaxScore, opt => opt.MapFrom(src => src.Essay != null && src.Essay.Assessment != null ? src.Essay.Assessment.TotalPoints : 0))
                .ForMember(dest => dest.Feedback, opt => opt.MapFrom(src => src.TeacherFeedback ?? src.Feedback ?? string.Empty))
                .ForMember(dest => dest.GradedAt, opt => opt.MapFrom(src => src.TeacherGradedAt ?? src.GradedAt ?? DateTime.UtcNow))
                .ForMember(dest => dest.GradedByTeacher, opt => opt.MapFrom(src => src.TeacherScore.HasValue))
                .ForMember(dest => dest.FinalScore, opt => opt.MapFrom(src => src.FinalScore))
                .ForMember(dest => dest.Breakdown, opt => opt.Ignore()) // Only from AI
                .ForMember(dest => dest.Strengths, opt => opt.Ignore()) // Only from AI
                .ForMember(dest => dest.Improvements, opt => opt.Ignore()); // Only from AI

            // Pronunciation mappings
            CreateMap<FlashCard, FlashCardWithPronunciationDto>()
                .ForMember(dest => dest.Word, opt => opt.MapFrom(src => src.Word))
                .ForMember(dest => dest.Definition, opt => opt.MapFrom(src => src.Meaning))
                .ForMember(dest => dest.Example, opt => opt.MapFrom(src => src.Example))
                .ForMember(dest => dest.Phonetic, opt => opt.MapFrom(src => src.Pronunciation))
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) // Set in service with BuildPublicUrl
                .ForMember(dest => dest.AudioUrl, opt => opt.Ignore()) // Set in service with BuildPublicUrl
                .ForMember(dest => dest.Progress, opt => opt.Ignore()); // Set in service from PronunciationProgress

            CreateMap<PronunciationProgress, PronunciationProgressSummary>()
                .ForMember(dest => dest.Status, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.StatusColor, opt => opt.Ignore()); // Calculated in service

            // Streak mapping
            CreateMap<Streak, StreakDto>()
                .ForMember(dest => dest.IsActiveToday, opt => opt.MapFrom<IsActiveTodayResolver>());

            // Payment mappings
            CreateMap<Payment, TransactionHistoryDto>()
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.Gateway.ToString()))
                .ForMember(dest => dest.ProductName, opt => opt.Ignore()); // Set in service after mapping

            CreateMap<Payment, TransactionDetailDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : "N/A"))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : "N/A"))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.Gateway.ToString()))
                .ForMember(dest => dest.ProductName, opt => opt.Ignore()); // Set in service after mapping

            // Course mappings - DTO sang Entity
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

            // Course mappings - Entity sang DTO
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

            // Mapping cho lấy danh sách khóa học system
            CreateMap<Course, SystemCoursesListResponseDto>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DescriptionMarkdown))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageKey))
                .ForMember(dest => dest.IsEnrolled, opt => opt.Ignore()); // Set trong service

            // Mapping cho lấy chi tiết khóa học
            CreateMap<Course, CourseDetailWithEnrollmentDto>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DescriptionMarkdown))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageKey))
                .ForMember(dest => dest.TotalLessons, opt => opt.MapFrom(src => src.Lessons.Count))
                .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons))
                .ForMember(dest => dest.IsEnrolled, opt => opt.Ignore()); // Set trong service

            CreateMap<Lesson, LessonSummaryDto>()
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.OrderIndex));

            // Mapping cho chi tiết khóa học giáo viên
            CreateMap<Course, TeacherCourseDetailDto>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DescriptionMarkdown))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageKey))
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher != null ? src.Teacher.FullName : "Unknown"))
                .ForMember(dest => dest.TotalLessons, opt => opt.MapFrom(src => src.Lessons.Count))
                .ForMember(dest => dest.TotalStudents, opt => opt.MapFrom(src => src.UserCourses.Count))
                .ForMember(dest => dest.TotalModules, opt => opt.MapFrom(src => src.Lessons.Sum(l => l.Modules.Count)))
                .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons.OrderBy(l => l.OrderIndex).ToList()));

            // Mapping cho khóa học đã đăng ký
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

            // Mapping cho cập nhật khóa học
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

            // Mapping cho lesson
            CreateMap<Lesson, LessonDto>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageKey));

            // Mapping cho lesson với tiến độ
            CreateMap<Lesson, LessonWithProgressDto>()
    .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageKey))
    .ForMember(dest => dest.CompletionPercentage, opt => opt.Ignore()) // Set in service
    .ForMember(dest => dest.IsCompleted, opt => opt.Ignore()) // Set in service
    .ForMember(dest => dest.CompletedModules, opt => opt.Ignore()) // Set in service
    .ForMember(dest => dest.TotalModules, opt => opt.Ignore()) // Set in service
    .ForMember(dest => dest.VideoProgressPercentage, opt => opt.Ignore()) // Set in service
    .ForMember(dest => dest.StartedAt, opt => opt.Ignore()) // Set in service
    .ForMember(dest => dest.CompletedAt, opt => opt.Ignore()); // Set in service

            // Mapping cho tạo lesson
            CreateMap<AdminCreateLessonDto, Lesson>()
                .ForMember(dest => dest.LessonId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ImageKey, opt => opt.Ignore()); // Set manually in service after commit

            // Mapping cho tạo lesson giáo viên
            CreateMap<TeacherCreateLessonDto, Lesson>()
                .ForMember(dest => dest.LessonId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ImageKey, opt => opt.Ignore()); // Set manually in service after commit

            // Mapping cho cập nhật lesson
            CreateMap<UpdateLessonDto, Lesson>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ImageKey, opt => opt.Ignore()) // Set manually in service after commit
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Module mappings
            CreateMap<Module, ModuleDto>()
                .ForMember(dest => dest.LessonTitle, opt => opt.MapFrom(src => src.Lesson != null ? src.Lesson.Title : string.Empty))
                .ForMember(dest => dest.LectureCount, opt => opt.MapFrom(src => src.Lectures.Count))
                .ForMember(dest => dest.FlashCardCount, opt => opt.MapFrom(src => src.FlashCards.Count))
                .ForMember(dest => dest.AssessmentCount, opt => opt.MapFrom(src => src.Assessments.Count))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageKey))
                .ForMember(dest => dest.ImageType, opt => opt.MapFrom(src => src.ImageType));

            CreateMap<Module, ListModuleDto>();

            CreateMap<CreateModuleDto, Module>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ImageKey, opt => opt.Ignore())
                .ForMember(dest => dest.ImageType, opt => opt.Ignore());

            CreateMap<UpdateModuleDto, Module>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ImageKey, opt => opt.Ignore())
                .ForMember(dest => dest.ImageType, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Module, ModuleWithProgressDto>()
                .ForMember(dest => dest.LessonTitle, opt => opt.MapFrom(src => src.Lesson != null ? src.Lesson.Title : string.Empty))
                .ForMember(dest => dest.LectureCount, opt => opt.MapFrom(src => src.Lectures.Count))
                .ForMember(dest => dest.FlashCardCount, opt => opt.MapFrom(src => src.FlashCards.Count))
                .ForMember(dest => dest.AssessmentCount, opt => opt.MapFrom(src => src.Assessments.Count))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageKey))
                .ForMember(dest => dest.ImageType, opt => opt.MapFrom(src => src.ImageType))
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
                .ForMember(dest => dest.AttachmentUrl, opt => opt.MapFrom(src => src.AttachmentKey)) // Will be replaced with URL in service
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                // Grading fields
                .ForMember(dest => dest.AiScore, opt => opt.MapFrom(src => src.Score))
                .ForMember(dest => dest.AiFeedback, opt => opt.MapFrom(src => src.Feedback))
                .ForMember(dest => dest.AiGradedAt, opt => opt.MapFrom(src => src.GradedAt))
                .ForMember(dest => dest.TeacherScore, opt => opt.MapFrom(src => src.TeacherScore))
                .ForMember(dest => dest.TeacherFeedback, opt => opt.MapFrom(src => src.TeacherFeedback))
                .ForMember(dest => dest.TeacherGradedAt, opt => opt.MapFrom(src => src.TeacherGradedAt))
                .ForMember(dest => dest.GradedByTeacherName, opt => opt.MapFrom(src => src.GradedByTeacher != null ? src.GradedByTeacher.FullName : null))
                .ForMember(dest => dest.FinalScore, opt => opt.MapFrom(src => src.FinalScore))
                .ForMember(dest => dest.MaxScore, opt => opt.MapFrom(src => src.Essay != null && src.Essay.Assessment != null ? src.Essay.Assessment.TotalPoints : (decimal?)null));

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
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.TotalPossibleScore, opt => opt.MapFrom(src => src.TotalPossibleScore));

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
            CreateMap<QuizAttempt, QuizAttemptDto>()
                .ForMember(dest => dest.EndTime, opt => opt.Ignore()); // Calculated in service

            CreateMap<QuizAttempt, QuizAttemptWithQuestionsDto>()
                .ForMember(dest => dest.QuizSections, opt => opt.Ignore())
                .ForMember(dest => dest.EndTime, opt => opt.Ignore());

            CreateMap<QuizAttempt, QuizAttemptResultDto>()
                .ForMember(dest => dest.Percentage, opt => opt.Ignore())
                .ForMember(dest => dest.IsPassed, opt => opt.Ignore())
                .ForMember(dest => dest.ScoresByQuestion, opt => opt.Ignore())
                .ForMember(dest => dest.CorrectAnswers, opt => opt.Ignore());

            CreateMap<QuizAttempt, QuizScoreDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Percentage, opt => opt.Ignore())
                .ForMember(dest => dest.IsPassed, opt => opt.Ignore());



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

    // Custom value resolver for IsActiveToday
    public class IsActiveTodayResolver : IValueResolver<Streak, StreakDto, bool>
    {
        public bool Resolve(Streak source, StreakDto destination, bool destMember, ResolutionContext context)
        {
            var today = DateTime.UtcNow.Date;
            var lastActivity = source.LastActivityDate?.Date;
            return lastActivity == today;
        }
    }
}
