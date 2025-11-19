using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.DTOs
{

    // DTO tạo khóa học mới (Admin) - Request
    public class AdminCreateCourseRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
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
        public string? Img { get; set; }
        public string? ImageType { get; set; }
        public int MaxStudent { get; set; }
        public CourseType Type { get; set; } = CourseType.Teacher;
    }

    // DTO tham gia khóa học Teacher
    public class JoinCourseTeacherDto
    {
        public int CourseId { get; set; }
    }



    // DTO response chung cho Course
    public class CourseResponseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
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
        public string? Img { get; set; }
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

    // DTO danh sách khóa học cho User
    public class UserCourseListResponseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public string? ImageType { get; set; }
        public CourseType Type { get; set; }
        public decimal? Price { get; set; }
        public bool IsEnrolled { get; set; }
        public int EnrollmentCount { get; set; }
        public int MaxStudent { get; set; }
        public bool IsFeatured { get; set; }
        public bool CanJoin { get; set; }
        public string TeacherName { get; set; } = string.Empty;
    }

    // DTO chi tiết khóa học
    public class CourseDetailResponseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public string? ImageType { get; set; }
        public CourseType Type { get; set; }
        public decimal? Price { get; set; }
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int LessonCount { get; set; }
        public bool IsEnrolled { get; set; }
        public List<LessonSummaryDto>? Lessons { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO tóm tắt Lesson (dùng trong CourseDetailResponseDto)
    public class LessonSummaryDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
    }

    // DTO hiển thị khóa học cho Teacher
    public class TeacherCourseResponseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public string? ImageType { get; set; }
        public CourseType Type { get; set; }
        public decimal? Price { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public int LessonCount { get; set; }
        public int StudentCount { get; set; }
        public int EnrollmentCount { get; set; }
        public int MaxStudent { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO tóm tắt khóa học cho User
    public class UserCourseSummaryDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public string? ImageType { get; set; }
        public decimal? Price { get; set; }
        public bool IsEnrolled { get; set; }
        public string TeacherName { get; set; } = string.Empty;
    }

    // DTO tổng quan khóa học cho Admin
    public class AdminCourseSummaryDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public CourseType Type { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int LessonCount { get; set; }
        public int StudentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO tham gia khóa học qua mã lớp học
    public class EnrollCourseByClassCodeDto
    {
        public string ClassCode { get; set; } = string.Empty;
    }
    // DTO cập nhật khóa học (Admin) - Request
    public class AdminUpdateCourseRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public string? ImageType { get; set; }
        public decimal? Price { get; set; }
        public int MaxStudent { get; set; } = 0;
        public bool IsFeatured { get; set; } = false;
        public CourseType Type { get; set; } = CourseType.System;
    }

    // DTO cập nhật khóa học (Teacher) - Request
    public class TeacherUpdateCourseRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public string? ImageType { get; set; }
        public int MaxStudent { get; set; } = 0;
        public CourseType Type { get; set; } = CourseType.Teacher;
    }
}
