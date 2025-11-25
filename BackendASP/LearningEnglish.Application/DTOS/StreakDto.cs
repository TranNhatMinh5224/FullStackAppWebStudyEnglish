using System;

namespace LearningEnglish.Application.DTOS
{
    public class StreakDto
    {
        public int UserId { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TotalActiveDays { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public DateTime? CurrentStreakStartDate { get; set; }
        public bool IsActiveToday { get; set; }
        public string StreakStatus { get; set; } // "Active", "Broken", "New"
    }

    public class StreakHistoryDto
    {
        public DateTime Date { get; set; }
        public bool WasActive { get; set; }
        public int StreakOnThatDay { get; set; }
    }

    public class StreakUpdateResultDto
    {
        public bool Success { get; set; }
        public int NewCurrentStreak { get; set; }
        public int NewLongestStreak { get; set; }
        public bool IsNewRecord { get; set; }
        public string Message { get; set; }
    }
}
