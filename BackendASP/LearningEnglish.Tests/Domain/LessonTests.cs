using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Tests.Domain;

public class LessonTests
{
    // ===== Property Tests =====
    [Fact]
    public void Lesson_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var lesson = new Lesson();

        // Assert - Kiểm tra các giá trị mặc định
        Assert.Equal(string.Empty, lesson.Title);
        Assert.Null(lesson.Description);
        Assert.Equal(0, lesson.OrderIndex);
        Assert.Null(lesson.ImageKey);
        Assert.Null(lesson.ImageType);
        Assert.NotNull(lesson.Modules);
        Assert.Empty(lesson.Modules);
        Assert.NotNull(lesson.LessonCompletions);
        Assert.Empty(lesson.LessonCompletions);
    }

    [Fact]
    public void Lesson_CreatedAt_IsSetToUtcNow()
    {
        // Arrange & Act
        var beforeCreation = DateTime.UtcNow;
        var lesson = new Lesson();
        var afterCreation = DateTime.UtcNow;

        // Assert - CreatedAt phải nằm trong khoảng thời gian tạo
        Assert.True(lesson.CreatedAt >= beforeCreation);
        Assert.True(lesson.CreatedAt <= afterCreation);
    }

    [Fact]
    public void Lesson_UpdatedAt_IsSetToUtcNow()
    {
        // Arrange & Act
        var beforeCreation = DateTime.UtcNow;
        var lesson = new Lesson();
        var afterCreation = DateTime.UtcNow;

        // Assert - UpdatedAt phải nằm trong khoảng thời gian tạo
        Assert.True(lesson.UpdatedAt >= beforeCreation);
        Assert.True(lesson.UpdatedAt <= afterCreation);
    }

    [Fact]
    public void Lesson_PropertiesCanBeSet()
    {
        // Arrange & Act
        var createdDate = DateTime.UtcNow;
        var updatedDate = DateTime.UtcNow.AddHours(1);

        var lesson = new Lesson
        {
            LessonId = 1,
            Title = "Introduction to English Grammar",
            Description = "Learn the basics of English grammar",
            CourseId = 5,
            OrderIndex = 1,
            ImageKey = "lesson-image-123",
            ImageType = "image/png",
            CreatedAt = createdDate,
            UpdatedAt = updatedDate
        };

        // Assert - Tất cả properties phải được set đúng
        Assert.Equal(1, lesson.LessonId);
        Assert.Equal("Introduction to English Grammar", lesson.Title);
        Assert.Equal("Learn the basics of English grammar", lesson.Description);
        Assert.Equal(5, lesson.CourseId);
        Assert.Equal(1, lesson.OrderIndex);
        Assert.Equal("lesson-image-123", lesson.ImageKey);
        Assert.Equal("image/png", lesson.ImageType);
        Assert.Equal(createdDate, lesson.CreatedAt);
        Assert.Equal(updatedDate, lesson.UpdatedAt);
    }

    [Fact]
    public void Lesson_NullableProperties_CanBeNull()
    {
        // Arrange & Act
        var lesson = new Lesson
        {
            Description = null,
            ImageKey = null,
            ImageType = null
        };

        // Assert - Các nullable properties có thể là null
        Assert.Null(lesson.Description);
        Assert.Null(lesson.ImageKey);
        Assert.Null(lesson.ImageType);
    }

    // ===== Title Tests =====
    [Fact]
    public void Lesson_Title_CanBeSet()
    {
        // Arrange
        var lesson = new Lesson();

        // Act
        lesson.Title = "Lesson Title";

        // Assert
        Assert.Equal("Lesson Title", lesson.Title);
    }

    [Fact]
    public void Lesson_Title_CanBeEmpty()
    {
        // Arrange & Act
        var lesson = new Lesson { Title = "" };

        // Assert
        Assert.Equal("", lesson.Title);
    }

    [Fact]
    public void Lesson_Title_CanContainSpecialCharacters()
    {
        // Arrange & Act
        var lesson = new Lesson { Title = "Lesson 1: Grammar & Vocabulary (Part A)" };

        // Assert
        Assert.Equal("Lesson 1: Grammar & Vocabulary (Part A)", lesson.Title);
    }

    [Fact]
    public void Lesson_Title_CanContainUnicode()
    {
        // Arrange & Act
        var lesson = new Lesson { Title = "Bài học Tiếng Việt - 日本語レッスン" };

        // Assert
        Assert.Equal("Bài học Tiếng Việt - 日本語レッスン", lesson.Title);
    }

    // ===== Description Tests =====
    [Fact]
    public void Lesson_Description_CanBeNull()
    {
        // Arrange & Act
        var lesson = new Lesson { Description = null };

        // Assert
        Assert.Null(lesson.Description);
    }

    [Fact]
    public void Lesson_Description_CanBeEmpty()
    {
        // Arrange & Act
        var lesson = new Lesson { Description = "" };

        // Assert
        Assert.Equal("", lesson.Description);
    }

    [Fact]
    public void Lesson_Description_CanBeLongText()
    {
        // Arrange - Mô tả dài
        var longDescription = new string('A', 1000);
        var lesson = new Lesson { Description = longDescription };

        // Act & Assert
        Assert.Equal(longDescription, lesson.Description);
        Assert.Equal(1000, lesson.Description.Length);
    }

    [Fact]
    public void Lesson_Description_CanContainMarkdown()
    {
        // Arrange & Act - Description có thể chứa markdown
        var lesson = new Lesson
        {
            Description = "# Lesson Overview\n\n- Point 1\n- Point 2\n\n**Bold text**"
        };

        // Assert
        Assert.Contains("# Lesson Overview", lesson.Description);
        Assert.Contains("**Bold text**", lesson.Description);
    }

    // ===== OrderIndex Tests =====
    [Fact]
    public void Lesson_OrderIndex_CanBeZero()
    {
        // Arrange & Act
        var lesson = new Lesson { OrderIndex = 0 };

        // Assert
        Assert.Equal(0, lesson.OrderIndex);
    }

    [Fact]
    public void Lesson_OrderIndex_CanBePositive()
    {
        // Arrange & Act
        var lesson = new Lesson { OrderIndex = 5 };

        // Assert
        Assert.Equal(5, lesson.OrderIndex);
    }

    [Fact]
    public void Lesson_OrderIndex_CanBeNegative()
    {
        // Arrange & Act - Negative index có thể dùng cho special cases
        var lesson = new Lesson { OrderIndex = -1 };

        // Assert
        Assert.Equal(-1, lesson.OrderIndex);
    }

    [Fact]
    public void Lesson_OrderIndex_CanBeLargeNumber()
    {
        // Arrange & Act
        var lesson = new Lesson { OrderIndex = 999 };

        // Assert
        Assert.Equal(999, lesson.OrderIndex);
    }

    // ===== Image Properties Tests =====
    [Fact]
    public void Lesson_ImageProperties_CanBeSet()
    {
        // Arrange & Act
        var lesson = new Lesson
        {
            ImageKey = "image-key-123",
            ImageType = "image/jpeg"
        };

        // Assert
        Assert.Equal("image-key-123", lesson.ImageKey);
        Assert.Equal("image/jpeg", lesson.ImageType);
    }

    [Fact]
    public void Lesson_ImageProperties_CanBeCleared()
    {
        // Arrange - Lesson có ảnh
        var lesson = new Lesson
        {
            ImageKey = "image-123",
            ImageType = "image/png"
        };
        Assert.NotNull(lesson.ImageKey);

        // Act - Xóa ảnh
        lesson.ImageKey = null;
        lesson.ImageType = null;

        // Assert - Không còn ảnh
        Assert.Null(lesson.ImageKey);
        Assert.Null(lesson.ImageType);
    }

    [Fact]
    public void Lesson_WithoutImage_HasNullImageProperties()
    {
        // Arrange & Act
        var lesson = new Lesson();

        // Assert - Mặc định không có ảnh
        Assert.Null(lesson.ImageKey);
        Assert.Null(lesson.ImageType);
    }

    [Fact]
    public void Lesson_ImageType_CanBeAnyMimeType()
    {
        // Arrange & Act - Test các mime types khác nhau
        var lesson1 = new Lesson { ImageType = "image/png" };
        var lesson2 = new Lesson { ImageType = "image/jpeg" };
        var lesson3 = new Lesson { ImageType = "image/webp" };
        var lesson4 = new Lesson { ImageType = "image/svg+xml" };

        // Assert
        Assert.Equal("image/png", lesson1.ImageType);
        Assert.Equal("image/jpeg", lesson2.ImageType);
        Assert.Equal("image/webp", lesson3.ImageType);
        Assert.Equal("image/svg+xml", lesson4.ImageType);
    }

    // ===== CourseId Tests =====
    [Fact]
    public void Lesson_CourseId_CanBeSet()
    {
        // Arrange & Act
        var lesson = new Lesson { CourseId = 10 };

        // Assert
        Assert.Equal(10, lesson.CourseId);
    }

    [Fact]
    public void Lesson_CourseId_DefaultIsZero()
    {
        // Arrange & Act
        var lesson = new Lesson();

        // Assert
        Assert.Equal(0, lesson.CourseId);
    }

    // ===== Navigation Properties Tests =====
    [Fact]
    public void Lesson_Modules_InitializedAsEmptyList()
    {
        // Arrange & Act
        var lesson = new Lesson();

        // Assert
        Assert.NotNull(lesson.Modules);
        Assert.Empty(lesson.Modules);
        Assert.IsType<List<Module>>(lesson.Modules);
    }

    [Fact]
    public void Lesson_Modules_CanAddModules()
    {
        // Arrange
        var lesson = new Lesson();
        var module1 = new Module { ModuleId = 1, Name = "Module 1" };
        var module2 = new Module { ModuleId = 2, Name = "Module 2" };

        // Act
        lesson.Modules.Add(module1);
        lesson.Modules.Add(module2);

        // Assert
        Assert.Equal(2, lesson.Modules.Count);
        Assert.Contains(module1, lesson.Modules);
        Assert.Contains(module2, lesson.Modules);
    }

    [Fact]
    public void Lesson_LessonCompletions_InitializedAsEmptyList()
    {
        // Arrange & Act
        var lesson = new Lesson();

        // Assert
        Assert.NotNull(lesson.LessonCompletions);
        Assert.Empty(lesson.LessonCompletions);
        Assert.IsType<List<LessonCompletion>>(lesson.LessonCompletions);
    }

    [Fact]
    public void Lesson_Course_CanBeNull()
    {
        // Arrange & Act
        var lesson = new Lesson();

        // Assert - Navigation property mặc định là null
        Assert.Null(lesson.Course);
    }

    [Fact]
    public void Lesson_Course_CanBeSet()
    {
        // Arrange
        var course = new Course { CourseId = 1, Title = "English Course" };
        var lesson = new Lesson { CourseId = 1 };

        // Act
        lesson.Course = course;

        // Assert
        Assert.NotNull(lesson.Course);
        Assert.Equal(course, lesson.Course);
        Assert.Equal(1, lesson.Course.CourseId);
    }

    // ===== Complex Scenario Tests =====
    [Fact]
    public void Lesson_WithMultipleModules_MaintainsOrder()
    {
        // Arrange
        var lesson = new Lesson { Title = "Main Lesson" };
        var modules = new List<Module>
        {
            new Module { ModuleId = 1, Name = "Module 1", OrderIndex = 1 },
            new Module { ModuleId = 2, Name = "Module 2", OrderIndex = 2 },
            new Module { ModuleId = 3, Name = "Module 3", OrderIndex = 3 }
        };

        // Act - Thêm modules theo thứ tự
        foreach (var module in modules)
        {
            lesson.Modules.Add(module);
        }

        // Assert - Thứ tự được giữ nguyên
        Assert.Equal(3, lesson.Modules.Count);
        Assert.Equal("Module 1", lesson.Modules[0].Name);
        Assert.Equal("Module 2", lesson.Modules[1].Name);
        Assert.Equal("Module 3", lesson.Modules[2].Name);
    }

    [Fact]
    public void Lesson_CanBeOrdered_ByOrderIndex()
    {
        // Arrange - Tạo nhiều lessons với OrderIndex khác nhau
        var lessons = new List<Lesson>
        {
            new Lesson { LessonId = 1, Title = "Lesson C", OrderIndex = 3 },
            new Lesson { LessonId = 2, Title = "Lesson A", OrderIndex = 1 },
            new Lesson { LessonId = 3, Title = "Lesson B", OrderIndex = 2 }
        };

        // Act - Sắp xếp theo OrderIndex
        var sortedLessons = lessons.OrderBy(l => l.OrderIndex).ToList();

        // Assert - Thứ tự đúng theo OrderIndex
        Assert.Equal("Lesson A", sortedLessons[0].Title);
        Assert.Equal("Lesson B", sortedLessons[1].Title);
        Assert.Equal("Lesson C", sortedLessons[2].Title);
    }

    [Fact]
    public void Lesson_UpdatedAt_CanBeModified()
    {
        // Arrange
        var lesson = new Lesson { Title = "Original" };
        var originalUpdatedAt = lesson.UpdatedAt;
        
        // Act - Simulate update after some time
        System.Threading.Thread.Sleep(10); // Small delay
        lesson.UpdatedAt = DateTime.UtcNow;
        lesson.Title = "Updated";

        // Assert - UpdatedAt should be later than original
        Assert.True(lesson.UpdatedAt > originalUpdatedAt);
        Assert.Equal("Updated", lesson.Title);
    }

    [Fact]
    public void Lesson_BelongsToCourse_Relationship()
    {
        // Arrange - Course với lessons
        var course = new Course 
        { 
            CourseId = 1, 
            Title = "English Course" 
        };
        
        var lesson1 = new Lesson 
        { 
            LessonId = 1, 
            Title = "Lesson 1", 
            CourseId = 1, 
            Course = course 
        };
        
        var lesson2 = new Lesson 
        { 
            LessonId = 2, 
            Title = "Lesson 2", 
            CourseId = 1, 
            Course = course 
        };

        // Act
        course.Lessons.Add(lesson1);
        course.Lessons.Add(lesson2);

        // Assert - Relationship được thiết lập đúng
        Assert.Equal(2, course.Lessons.Count);
        Assert.Equal(course, lesson1.Course);
        Assert.Equal(course, lesson2.Course);
        Assert.Equal(course.CourseId, lesson1.CourseId);
        Assert.Equal(course.CourseId, lesson2.CourseId);
    }

    [Fact]
    public void Lesson_WithAllProperties_CreatesCompleteLesson()
    {
        // Arrange & Act - Tạo lesson hoàn chỉnh với tất cả properties
        var course = new Course { CourseId = 5, Title = "Test Course" };
        var lesson = new Lesson
        {
            LessonId = 10,
            Title = "Complete Lesson",
            Description = "This is a complete lesson with all properties set",
            CourseId = 5,
            Course = course,
            OrderIndex = 1,
            ImageKey = "lesson-image",
            ImageType = "image/jpeg",
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            UpdatedAt = DateTime.UtcNow
        };

        lesson.Modules.Add(new Module { ModuleId = 1, Name = "Module 1" });
        lesson.Modules.Add(new Module { ModuleId = 2, Name = "Module 2" });

        // Assert - Tất cả đều đúng
        Assert.Equal(10, lesson.LessonId);
        Assert.Equal("Complete Lesson", lesson.Title);
        Assert.NotNull(lesson.Description);
        Assert.Equal(5, lesson.CourseId);
        Assert.NotNull(lesson.Course);
        Assert.Equal(1, lesson.OrderIndex);
        Assert.NotNull(lesson.ImageKey);
        Assert.NotNull(lesson.ImageType);
        Assert.Equal(2, lesson.Modules.Count);
        Assert.True(lesson.CreatedAt < lesson.UpdatedAt);
    }

    [Fact]
    public void Lesson_EmptyModules_DoesNotAffectLesson()
    {
        // Arrange & Act - Lesson không có modules
        var lesson = new Lesson
        {
            Title = "Lesson without modules",
            CourseId = 1
        };

        // Assert - Lesson vẫn valid
        Assert.Equal("Lesson without modules", lesson.Title);
        Assert.Empty(lesson.Modules);
        Assert.NotNull(lesson.Modules);
    }

    [Fact]
    public void Lesson_ComparisonByOrderIndex_WorksCorrectly()
    {
        // Arrange
        var lesson1 = new Lesson { OrderIndex = 1 };
        var lesson2 = new Lesson { OrderIndex = 2 };
        var lesson3 = new Lesson { OrderIndex = 1 };

        // Act & Assert - So sánh OrderIndex
        Assert.True(lesson1.OrderIndex < lesson2.OrderIndex);
        Assert.True(lesson2.OrderIndex > lesson1.OrderIndex);
        Assert.Equal(lesson1.OrderIndex, lesson3.OrderIndex);
    }
}
