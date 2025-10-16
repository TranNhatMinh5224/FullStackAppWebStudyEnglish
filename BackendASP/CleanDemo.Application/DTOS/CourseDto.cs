using CleanDemo.Domain.Enums;

namespace CleanDemo.Application.DTOs
{


    // DTO tạo khóa học mới (Admin) - Request
    public class AdminCreateCourseRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public CourseType Type { get; set; } = CourseType.System;
    }

    // DTO tạo khóa học mới (Teacher) - Request
    public class TeacherCreateCourseRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public CourseType Type { get; set; } = CourseType.Teacher;
        public decimal? Price { get; set; }
    }

    // === RESPONSE DTOs ===

    // DTO response chung cho Course
    public class CourseResponseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public CourseType Type { get; set; }
        public decimal? Price { get; set; }
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int LessonCount { get; set; }
        public int StudentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO danh sách khóa học cho Admin
    public class AdminCourseListResponseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public CourseType Type { get; set; }
        public decimal? Price { get; set; }
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int LessonCount { get; set; }
        public int StudentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO danh sách khóa học cho User
    public class UserCourseListResponseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public decimal? Price { get; set; }
        public bool IsEnrolled { get; set; }
        public string TeacherName { get; set; } = string.Empty;
    }

    // DTO chi tiết khóa học
    public class CourseDetailResponseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public CourseType Type { get; set; }
        public decimal? Price { get; set; }
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int LessonCount { get; set; }
        public bool IsEnrolled { get; set; }
        public List<LessonResponseDto>? Lessons { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO cho Lesson (dùng trong CourseDetailResponseDto)
    public class LessonResponseDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
    }



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
    public class TeacherCreateCourseDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public CourseType Type { get; set; } = CourseType.Teacher;
        public decimal? Price { get; set; }
        public int TeacherId { get; set; }
    }

    // DTO tạo khóa học mới (Admin) - Không cần TeacherId, Price
    public class AdminCreateCourseDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Img { get; set; }
        public decimal? Price { get; set; }
        public CourseType Type { get; set; } = CourseType.System; // Mặc định là System (Admin)
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



    // DTO tham gia khóa học Teacher
    public class JoinCourseTeacherDto
    {
        public int CourseId { get; set; }
    }

    // DTO đăng ký khóa học
    public class EnrollCourseDto
    {
        public int CourseId { get; set; }
    }
}
