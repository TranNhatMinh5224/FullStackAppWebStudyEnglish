using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.DTOs
{
    // DTO tạo khóa học mới (Admin) - Request
    public class AdminCreateCourseRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageTempKey { get; set; }
        public string? ImageType { get; set; }
        public decimal? Price { get; set; }
        public int MaxStudent { get; set; }
        public bool IsFeatured { get; set; } = false;
        public CourseType Type { get; set; } = CourseType.System;
    }

    // DTO tạo khóa học mới (Teacher) - Request
    public class TeacherCreateCourseRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageTempKey { get; set; }
        public string? ImageType { get; set; }
        public int MaxStudent { get; set; }
        public CourseType Type { get; set; } = CourseType.Teacher;
    }

    // DTO response chung cho Course
    public class CourseResponseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? ImageType { get; set; }
        public CourseType Type { get; set; }
        public decimal? Price { get; set; }
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int LessonCount { get; set; }
        public int StudentCount { get; set; }
        public string? ClassCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public int EnrollmentCount { get; set; }
        public int MaxStudent { get; set; }
        public bool IsFeatured { get; set; }
    }

    // DTO danh sách khóa học cho Admin
    public class AdminCourseListResponseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? ImageType { get; set; }
        public CourseType Type { get; set; }
        public decimal? Price { get; set; }
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int LessonCount { get; set; }
        public int StudentCount { get; set; }
        public int EnrollmentCount { get; set; }
        public int MaxStudent { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO cho endpoint: GET /api/user/courses/system-courses
    // Response: Danh sách các khóa học system với trạng thái enrollment
    public class SystemCoursesListResponseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal? Price { get; set; }
        public bool IsEnrolled { get; set; }
        public int EnrollmentCount { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO cho endpoint: GET /api/user/courses/{courseId}
    // Response: Chi tiết khóa học với trạng thái enrollment, progress và danh sách lessons
    public class CourseDetailWithEnrollmentDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal? Price { get; set; }
        public bool IsEnrolled { get; set; }
        public int EnrollmentCount { get; set; }
        public bool IsFeatured { get; set; }
        public int TotalLessons { get; set; }
        public int MaxStudent { get; set; }
        public List<LessonSummaryDto> Lessons { get; set; } = new();
        public DateTime CreatedAt { get; set; }

        // Progress information (for enrolled users)
        public decimal ProgressPercentage { get; set; } = 0;
        public int CompletedLessons { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;
        public DateTime? EnrolledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    // DTO tóm tắt Lesson (dùng trong CourseDetailWithEnrollmentDto)
    public class LessonSummaryDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
    }

    // DTO khóa học đã đăng ký với tiến độ (My Enrolled Courses)
    public class EnrolledCourseWithProgressDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? ImageType { get; set; }
        public CourseType Type { get; set; }
        public decimal? Price { get; set; }
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int LessonCount { get; set; }
        public int StudentCount { get; set; }

        // Progress information
        public decimal ProgressPercentage { get; set; } = 0;
        public int CompletedLessons { get; set; } = 0;
        public int TotalLessons { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;
        public DateTime EnrolledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    // DTO tham gia khóa học qua mã lớp học
    public class EnrollCourseByClassCodeDto
    {
        public string ClassCode { get; set; } = string.Empty;
    }

    // DTO cập nhật khóa học (Admin) - Hỗ trợ PARTIAL UPDATE
    public class AdminUpdateCourseRequestDto
    {
        // Nullable để phân biệt: không gửi vs gửi giá trị rỗng
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageTempKey { get; set; }
        public string? ImageType { get; set; }
        public decimal? Price { get; set; }
        public int? MaxStudent { get; set; }
        public bool? IsFeatured { get; set; }
    }

    // DTO cập nhật khóa học (Teacher) - Request
    public class TeacherUpdateCourseRequestDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageTempKey { get; set; }
        public string? ImageType { get; set; }
        public int? MaxStudent { get; set; }
    }

    // DTO chi tiết khóa học cho Teacher với thông tin đầy đủ về lessons và students
    public class TeacherCourseDetailDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? ImageType { get; set; }
        public CourseType Type { get; set; }
        public decimal? Price { get; set; }
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string? ClassCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MaxStudent { get; set; }
        public bool IsFeatured { get; set; }
        
        // Statistics
        public int TotalLessons { get; set; }
        public int TotalStudents { get; set; }
        public int TotalModules { get; set; }
        
        // Lessons list
        public List<LessonSummaryDto> Lessons { get; set; } = new();
    }
}
