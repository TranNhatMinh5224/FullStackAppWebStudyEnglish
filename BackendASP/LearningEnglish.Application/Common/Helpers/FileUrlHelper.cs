using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Common.Helpers
{
    // Helper để chuyển KEY → URL cho các DTO response
    public static class FileUrlHelper
    {
        // Set ImageUrl cho danh sách Course (dùng generic method)
        public static async Task SetImageUrlForCourses(
            IEnumerable<Course> courses,
            IEnumerable<CourseResponseDto> courseDtos,
            IFileStorageService fileStorageService)
        {
            await SetImageUrlForList(
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
        public static async Task SetImageUrlForAdminCourseList(
            IEnumerable<Course> courses,
            IEnumerable<AdminCourseListResponseDto> courseDtos,
            IFileStorageService fileStorageService)
        {
            await SetImageUrlForList(
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
        public static async Task SetImageUrlForUserCourseList(
            IEnumerable<Course> courses,
            IEnumerable<UserCourseListResponseDto> courseDtos,
            IFileStorageService fileStorageService)
        {
            await SetImageUrlForList(
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
        public static async Task SetImageUrlForCourse(
            Course course,
            CourseResponseDto courseDto,
            IFileStorageService fileStorageService)
        {
            await SetImageUrlForSingle(
                course,
                courseDto,
                fileStorageService,
                c => c.ImageUrl,
                (d, url) => d.ImageUrl = url
            );
        }

        // Set ImageUrl cho danh sách Lesson (dùng generic method)
        public static async Task SetImageUrlForLessons(
            IEnumerable<Lesson> lessons,
            IEnumerable<LessonDto> lessonDtos,
            IFileStorageService fileStorageService)
        {
            await SetImageUrlForList(
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
        public static async Task SetImageUrlForListLessons(
            IEnumerable<Lesson> lessons,
            IEnumerable<ListLessonDto> lessonDtos,
            IFileStorageService fileStorageService)
        {
            await SetImageUrlForList(
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
        public static async Task SetImageUrlForLesson(
            Lesson lesson,
            LessonDto lessonDto,
            IFileStorageService fileStorageService)
        {
            await SetImageUrlForSingle(
                lesson,
                lessonDto,
                fileStorageService,
                l => l.ImageUrl,
                (d, url) => d.ImageUrl = url
            );
        }

        // Generic method cho các entity khác (nếu cần)
        public static async Task SetImageUrlForList<TEntity, TDto>(
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
                        var urlResponse = await fileStorageService.GetFileUrl(imageKey);
                        if (urlResponse.Success && urlResponse.Data != null)
                        {
                            setImageUrl(dto, urlResponse.Data);
                        }
                    }
                }
            }
        }

        // Generic method cho single entity
        public static async Task SetImageUrlForSingle<TEntity, TDto>(
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
                    var urlResponse = await fileStorageService.GetFileUrl(imageKey);
                    if (urlResponse.Success && urlResponse.Data != null)
                    {
                        setImageUrl(dto, urlResponse.Data);
                    }
                }
            }
        }
    }
}

