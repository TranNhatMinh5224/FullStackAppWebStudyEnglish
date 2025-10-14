using CleanDemo.Domain.Enums;

namespace CleanDemo.Application.DTOs
{
    // DTO hiển thị tất cả khóa học quản lý dành cho admin 
    public class CourseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public CourseType Type { get; set; }
        public decimal? Price { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int LessonCount { get; set; }
        public int StudentCount { get; set; }
    }

    // DTO tạo khóa học mới
    public class CreateCourseDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public CourseType Type { get; set; } = CourseType.System;
        public decimal? Price { get; set; }
        public int TeacherId { get; set; }
    }

    // DTO danh sách khóa học System trang chủ User
    public class UserCourseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public decimal? Price { get; set; }
        public bool IsEnrolled { get; set; } 
    }

    // DTO danh sách khóa học của Teacher (quản lý)
    public class ListMyCourseTeacherDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public CourseType Type { get; set; }
        public int StudentCount { get; set; }
    }

    // DTO danh sách khóa học Student đã tham gia và Course teacher
    public class ListMyCourseStudentDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public CourseType Type { get; set; }
        public string TeacherName { get; set; } = string.Empty;
    }

    // DTO chi tiết khóa học
    public class CourseDetailDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public CourseType Type { get; set; }
        public decimal? Price { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int LessonCount { get; set; }
        public bool IsEnrolled { get; set; } 
        public List<LessonDto>? Lessons { get; set; } 
    }

    // DTO đăng ký khóa học
    public class EnrollCourseDto
    {
        public int CourseId { get; set; }
    }

    // DTO tham gia khóa học Teacher
    public class JoinCourseTeacherDto
    {
        public int CourseId { get; set; }
    }

    // DTO cho Lesson (dùng trong CourseDetailDto)
    public class LessonDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
