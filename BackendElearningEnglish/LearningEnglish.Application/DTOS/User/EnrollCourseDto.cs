namespace LearningEnglish.Application.DTOs
{
    // DTO đăng ký khóa học
    public class EnrollCourseDto
    {
        public int CourseId { get; set; }
    }

    // DTO cho Teacher/Admin thêm học sinh vào course bằng email
    public class AddStudentToCourseDto
    {
        public string Email { get; set; } = string.Empty;
    }
}
