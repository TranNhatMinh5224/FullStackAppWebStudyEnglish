using System;
using System.Collections.Generic;

namespace LearningEnglish.Application.DTOs
{
    public class UserProgressDashboardDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<CourseProgressDetailDto> Courses { get; set; } = new();
        public ProgressStatisticsDto Statistics { get; set; } = new ProgressStatisticsDto();
    }

    public class CourseProgressDetailDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseDescription { get; set; } = string.Empty;
        public decimal ProgressPercentage { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime EnrolledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? LastAccessedAt { get; set; }
        public List<LessonProgressDetailDto> Lessons { get; set; } = new();
    }

    public class LessonProgressDetailDto
    {
        public int LessonId { get; set; }
        public string LessonName { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public decimal CompletionPercentage { get; set; }
        public int CompletedModules { get; set; }
        public int TotalModules { get; set; }
        public decimal VideoProgressPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<ModuleProgressDetailDto> Modules { get; set; } = new();
    }

    public class ModuleProgressDetailDto
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleType { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public bool IsCompleted { get; set; }
        public decimal ProgressPercentage { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class ProgressStatisticsDto
    {
        public int TotalCoursesEnrolled { get; set; }
        public int TotalCoursesCompleted { get; set; }
        public int TotalLessonsCompleted { get; set; }
        public int TotalModulesCompleted { get; set; }
        public int TotalQuizzesTaken { get; set; }
        public decimal AverageQuizScore { get; set; }
        public int TotalFlashcardsReviewed { get; set; }
        public int CurrentReviewStreak { get; set; }
        public int TotalPronunciationAttempts { get; set; }
        public decimal AveragePronunciationScore { get; set; }
    }

    public class CourseProgressSummaryDto
    {
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public decimal ProgressPercentage { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? LastAccessedAt { get; set; }
    }

    public class LessonProgressSummaryDto
    {
        public string LessonId { get; set; } = string.Empty;
        public string LessonName { get; set; } = string.Empty;
        public decimal CompletionPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? StartedAt { get; set; }
    }
}
