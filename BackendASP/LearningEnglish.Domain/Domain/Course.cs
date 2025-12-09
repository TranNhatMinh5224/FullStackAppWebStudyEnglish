using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Domain.Entities
{


    public class Course
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DescriptionMarkdown { get; set; } = string.Empty;
        public string? ImageKey { get; set; }
        public string? ImageType { get; set; }
        public CourseType Type { get; set; } = CourseType.System;
        public CourseStatus Status { get; set; } = CourseStatus.Draft;
        public decimal? Price { get; set; }
        public int? TeacherId { get; set; }
        public string? ClassCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int EnrollmentCount { get; set; } = 0;
        public int MaxStudent { get; set; } = 0;
        public bool IsFeatured { get; set; } = false;

        // Navigation Properties
        public User? Teacher { get; set; }
        public List<Lesson> Lessons { get; set; } = new();
        public List<UserCourse> UserCourses { get; set; } = new();
        public List<CourseProgress> CourseProgresses { get; set; } = new();




        public bool IsFree()
        {
            return Price == null || Price == 0;
        }
        public bool CanJoin()
        {
            return MaxStudent == 0 || EnrollmentCount < MaxStudent;
        }
        public void EnrollStudent()
        {
            if (CanJoin())
            {
                EnrollmentCount++;
            }
            else
            {
                throw new InvalidOperationException("Cannot enroll more students, maximum capacity reached.");
            }
        }
        public void UnenrollStudent()
        {
            if (EnrollmentCount > 0)
            {
                EnrollmentCount--;
            }
        }


    }
}


