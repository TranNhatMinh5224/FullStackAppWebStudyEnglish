namespace LearningEnglish.Application.Configurations
{
    // Configuration options for Spaced Repetition Algorithm
    // Giá trị mặc định CỰC THẤP để dễ test - sau khi test OK hãy tăng lên trong code này
    public class SpacedRepetitionOptions
    {
        // Khoảng cách ngày tối thiểu để coi như đã thuộc từ
        // Mặc định: 1 ngày (CỰC THẤP để test nhanh) - Production nên dùng 60 ngày
        public int MasteryIntervalDays { get; set; } = 1;

        // Số lần ôn tối thiểu để coi như đã thuộc
        // Mặc định: 1 lần (CỰC THẤP để test nhanh) - Production nên dùng 5 lần
        public int MasteryMinimumRepetitions { get; set; } = 1;

        // Khoảng cách ngày để coi như gần thuộc (dùng cho thống kê)
        // Mặc định: 2 ngày (CỰC THẤP để test nhanh) - Production nên dùng 21 ngày
        public int NearMasteryIntervalDays { get; set; } = 2;

        // Quality tối thiểu để tính là ôn tập thành công
        // Mặc định: 3 (>= 3 là pass)
        public int MinimumPassQuality { get; set; } = 3;
    }
}
