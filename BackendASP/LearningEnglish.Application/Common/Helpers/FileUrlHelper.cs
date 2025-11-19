using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Common.Helpers
{
    // Helper để chuyển KEY → URL cho các DTO response
    public static class FileUrlHelper
    {
        // Set ImageUrl cho danh sách Course (dùng generic method)
        public static void SetImageUrlForCourses(
            IEnumerable<Course> courses,
            IEnumerable<CourseResponseDto> courseDtos,
            IFileStorageService fileStorageService)
        {
            SetImageUrlForList(
                courses,
                courseDtos,
                fileStorageService,
                c => c.CourseId,
                d => d.CourseId,
                c => c.ImageUrl,
                (d, url) => d.ImageUrl = url
            );
        }

        // Set ImageUrl cho danh sách Course (AdminCourseListResponseDto) - dùng generic method
        public static void SetImageUrlForAdminCourseList(
            IEnumerable<Course> courses,
            IEnumerable<AdminCourseListResponseDto> courseDtos,
            IFileStorageService fileStorageService)
        {
            SetImageUrlForList(
                courses,
                courseDtos,
                fileStorageService,
                c => c.CourseId,
                d => d.CourseId,
                c => c.ImageUrl,
                (d, url) => d.ImageUrl = url
            );
        }

        // Set ImageUrl cho danh sách Course (UserCourseListResponseDto) - dùng generic method
        public static void SetImageUrlForUserCourseList(
            IEnumerable<Course> courses,
            IEnumerable<UserCourseListResponseDto> courseDtos,
            IFileStorageService fileStorageService)
        {
            SetImageUrlForList(
                courses,
                courseDtos,
                fileStorageService,
                c => c.CourseId,
                d => d.CourseId,
                c => c.ImageUrl,
                (d, url) => d.ImageUrl = url
            );
        }

        // Set ImageUrl cho single Course (dùng generic method)
        public static void SetImageUrlForCourse(
            Course course,
            CourseResponseDto courseDto,
            IFileStorageService fileStorageService)
        {
            SetImageUrlForSingle(
                course,
                courseDto,
                fileStorageService,
                c => c.ImageUrl,
                (d, url) => d.ImageUrl = url
            );
        }

        // Set ImageUrl cho danh sách Lesson (dùng generic method)
        public static void SetImageUrlForLessons(
            IEnumerable<Lesson> lessons,
            IEnumerable<LessonDto> lessonDtos,
            IFileStorageService fileStorageService)
        {
            SetImageUrlForList(
                lessons,
                lessonDtos,
                fileStorageService,
                l => l.LessonId,
                d => d.LessonId,
                l => l.ImageUrl,
                (d, url) => d.ImageUrl = url
            );
        }

        // Set ImageUrl cho danh sách Lesson (ListLessonDto) - dùng generic method
        public static void SetImageUrlForListLessons(
            IEnumerable<Lesson> lessons,
            IEnumerable<ListLessonDto> lessonDtos,
            IFileStorageService fileStorageService)
        {
            SetImageUrlForList(
                lessons,
                lessonDtos,
                fileStorageService,
                l => l.LessonId,
                d => d.LessonId,
                l => l.ImageUrl,
                (d, url) => d.ImageUrl = url
            );
        }

        // Set ImageUrl cho single Lesson (dùng generic method)
        public static void SetImageUrlForLesson(
            Lesson lesson,
            LessonDto lessonDto,
            IFileStorageService fileStorageService)
        {
            SetImageUrlForSingle(
                lesson,
                lessonDto,
                fileStorageService,
                l => l.ImageUrl,
                (d, url) => d.ImageUrl = url
            );
        }

        // Generic method cho các entity khác (nếu cần)
        public static void SetImageUrlForList<TEntity, TDto>(
            IEnumerable<TEntity> entities,
            IEnumerable<TDto> dtos,
            IFileStorageService fileStorageService,
            Func<TEntity, int> getEntityId,
            Func<TDto, int> getDtoId,
            Func<TEntity, string?> getImageKey,
            Action<TDto, string> setImageUrl)
        {
            foreach (var dto in dtos)
            {
                var dtoId = getDtoId(dto);
                var entity = entities.FirstOrDefault(e => getEntityId(e) == dtoId);
                
                if (entity != null)
                {
                    var imageKey = getImageKey(entity);
                    if (!string.IsNullOrEmpty(imageKey))
                    {
                        var urlResponse = fileStorageService.GetFileUrl(imageKey);
                        if (urlResponse.Success && urlResponse.Data != null)
                        {
                            setImageUrl(dto, urlResponse.Data);
                        }
                    }
                }
            }
        }

        // Generic method cho single entity
        public static void SetImageUrlForSingle<TEntity, TDto>(
            TEntity entity,
            TDto dto,
            IFileStorageService fileStorageService,
            Func<TEntity, string?> getImageKey,
            Action<TDto, string> setImageUrl)
        {
            if (entity != null)
            {
                var imageKey = getImageKey(entity);
                if (!string.IsNullOrEmpty(imageKey))
                {
                    var urlResponse = fileStorageService.GetFileUrl(imageKey);
                    if (urlResponse.Success && urlResponse.Data != null)
                    {
                        setImageUrl(dto, urlResponse.Data);
                    }
                }
            }
        }
    }
}

