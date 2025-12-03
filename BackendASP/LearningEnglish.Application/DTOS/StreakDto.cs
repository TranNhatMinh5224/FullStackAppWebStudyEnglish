using System;

namespace LearningEnglish.Application.DTOs
{
    public class StreakDto
    {
        public int UserId { get; set; }
        public int CurrentStreak { get; set; } // Chuỗi ngày học liên tục hiện tại
        public DateTime? LastActivityDate { get; set; } // Ngày hoạt động cuối cùng
        public bool IsActiveToday { get; set; } // Đã học hôm nay chưa
    }

    public class StreakUpdateResultDto
    {
        public bool Success { get; set; }
        public int NewCurrentStreak { get; set; }
        public int NewLongestStreak { get; set; }
        public bool IsNewRecord { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
