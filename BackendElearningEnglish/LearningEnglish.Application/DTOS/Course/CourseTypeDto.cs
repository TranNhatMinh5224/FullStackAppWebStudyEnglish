namespace LearningEnglish.Application.DTOs
{
    // DTO cho loại khóa học
    public class CourseTypeDto
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
