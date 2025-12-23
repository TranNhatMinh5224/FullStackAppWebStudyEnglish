using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Moq;
using Microsoft.Extensions.Logging;
using AutoMapper;
using LearningEnglish.Application.Common.Helpers;
using Microsoft.Extensions.Configuration;

namespace LearningEnglish.Tests.Application;

public class LessonServiceTests
{
    private readonly Mock<ILessonRepository> _lessonRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<ILogger<LessonService>> _loggerMock;
    private readonly Mock<ITeacherPackageRepository> _teacherPackageRepositoryMock;
    private readonly Mock<IMinioFileStorage> _minioFileStorageMock;
    private readonly Mock<ILessonCompletionRepository> _lessonCompletionRepositoryMock;
    private readonly LessonService _lessonService;

    public LessonServiceTests()
    {
        // Cấu hình BuildPublicUrl cho tests
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Minio:BaseUrl"]).Returns("http://localhost:9000");
        BuildPublicUrl.Configure(configMock.Object);

        _lessonRepositoryMock = new Mock<ILessonRepository>();
        _mapperMock = new Mock<IMapper>();
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _loggerMock = new Mock<ILogger<LessonService>>();
        _teacherPackageRepositoryMock = new Mock<ITeacherPackageRepository>();
        _minioFileStorageMock = new Mock<IMinioFileStorage>();
        _lessonCompletionRepositoryMock = new Mock<ILessonCompletionRepository>();

        _lessonService = new LessonService(
            _lessonRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _courseRepositoryMock.Object,
            _teacherPackageRepositoryMock.Object,
            _minioFileStorageMock.Object,
            _lessonCompletionRepositoryMock.Object
        );
    }

    #region AdminAddLesson Tests

    [Fact]
    public async Task AdminAddLesson_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new AdminCreateLessonDto
        {
            Title = "Lesson 1",
            Description = "Test Description",
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Test Course",
            Type = CourseType.System,
            Status = CourseStatus.Published
        };

        var lesson = new Lesson
        {
            LessonId = 1,
            Title = dto.Title,
            Description = dto.Description,
            CourseId = dto.CourseId
        };

        var lessonDto = new LessonDto
        {
            LessonId = 1,
            Title = dto.Title,
            Description = dto.Description,
            CourseId = dto.CourseId
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(dto.CourseId))
            .ReturnsAsync(course);

        _lessonRepositoryMock
            .Setup(x => x.LessonIncourse(dto.Title, dto.CourseId))
            .ReturnsAsync(false);

        _lessonRepositoryMock
            .Setup(x => x.AddLesson(It.IsAny<Lesson>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(x => x.Map<LessonDto>(It.IsAny<Lesson>()))
            .Returns(lessonDto);

        // Act
        var result = await _lessonService.AdminAddLesson(dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode); // Create operations return 201
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Title, result.Data.Title);

        _courseRepositoryMock.Verify(x => x.GetCourseById(dto.CourseId), Times.Once);
        _lessonRepositoryMock.Verify(x => x.LessonIncourse(dto.Title, dto.CourseId), Times.Once);
        _lessonRepositoryMock.Verify(x => x.AddLesson(It.IsAny<Lesson>()), Times.Once);
    }

    [Fact]
    public async Task AdminAddLesson_WithNonExistentCourse_ReturnsNotFound()
    {
        // Arrange
        var dto = new AdminCreateLessonDto
        {
            Title = "Lesson 1",
            Description = "Test Description",
            CourseId = 999
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(dto.CourseId))
            .ReturnsAsync((Course?)null);

        // Act
        var result = await _lessonService.AdminAddLesson(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy khóa học", result.Message);
        Assert.Null(result.Data);

        _lessonRepositoryMock.Verify(x => x.AddLesson(It.IsAny<Lesson>()), Times.Never);
    }

    [Fact]
    public async Task AdminAddLesson_WithNonSystemCourse_ReturnsForbidden()
    {
        // Arrange
        var dto = new AdminCreateLessonDto
        {
            Title = "Lesson 1",
            Description = "Test Description",
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Teacher Course",
            Type = CourseType.Teacher,
            Status = CourseStatus.Published
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(dto.CourseId))
            .ReturnsAsync(course);

        // Act
        var result = await _lessonService.AdminAddLesson(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Chỉ admin mới có thể thêm bài học vào khóa học hệ thống", result.Message);

        _lessonRepositoryMock.Verify(x => x.AddLesson(It.IsAny<Lesson>()), Times.Never);
    }

    [Fact]
    public async Task AdminAddLesson_WithDuplicateTitle_ReturnsBadRequest()
    {
        // Arrange
        var dto = new AdminCreateLessonDto
        {
            Title = "Existing Lesson",
            Description = "Test Description",
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Test Course",
            Type = CourseType.System,
            Status = CourseStatus.Published
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(dto.CourseId))
            .ReturnsAsync(course);

        _lessonRepositoryMock
            .Setup(x => x.LessonIncourse(dto.Title, dto.CourseId))
            .ReturnsAsync(true); // Lesson already exists

        // Act
        var result = await _lessonService.AdminAddLesson(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Bài học đã tồn tại", result.Message);

        _lessonRepositoryMock.Verify(x => x.AddLesson(It.IsAny<Lesson>()), Times.Never);
    }

    [Fact]
    public async Task AdminAddLesson_WithImageTempKey_CommitsImageAndSavesKey()
    {
        // Arrange
        var dto = new AdminCreateLessonDto
        {
            Title = "Lesson 1",
            Description = "Test Description",
            CourseId = 1,
            ImageTempKey = "temp/image-123",
            ImageType = "image/jpeg"
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Test Course",
            Type = CourseType.System,
            Status = CourseStatus.Published
        };

        var lesson = new Lesson
        {
            LessonId = 1,
            Title = dto.Title,
            Description = dto.Description,
            CourseId = dto.CourseId,
            ImageKey = "lessons/real/image-123",
            ImageType = dto.ImageType
        };

        var lessonDto = new LessonDto
        {
            LessonId = 1,
            Title = dto.Title,
            ImageType = dto.ImageType
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(dto.CourseId))
            .ReturnsAsync(course);

        _lessonRepositoryMock
            .Setup(x => x.LessonIncourse(dto.Title, dto.CourseId))
            .ReturnsAsync(false);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.ImageTempKey, "lessons", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = "lessons/real/image-123"
            });

        _lessonRepositoryMock
            .Setup(x => x.AddLesson(It.Is<Lesson>(l => l.ImageKey == "lessons/real/image-123")))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(x => x.Map<LessonDto>(It.IsAny<Lesson>()))
            .Returns(lessonDto);

        // Act
        var result = await _lessonService.AdminAddLesson(dto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.ImageUrl); // ImageUrl should be built from ImageKey

        _minioFileStorageMock.Verify(x => x.CommitFileAsync(dto.ImageTempKey, "lessons", "real"), Times.Once);
    }

    [Fact]
    public async Task AdminAddLesson_WithImageCommitFailure_ReturnsError()
    {
        // Arrange
        var dto = new AdminCreateLessonDto
        {
            Title = "Lesson 1",
            Description = "Test Description",
            CourseId = 1,
            ImageTempKey = "temp/image-123"
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Test Course",
            Type = CourseType.System,
            Status = CourseStatus.Published
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(dto.CourseId))
            .ReturnsAsync(course);

        _lessonRepositoryMock
            .Setup(x => x.LessonIncourse(dto.Title, dto.CourseId))
            .ReturnsAsync(false);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.ImageTempKey, "lessons", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = false,
                Message = "Failed to commit file"
            });

        // Act
        var result = await _lessonService.AdminAddLesson(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Không thể lưu ảnh", result.Message);

        _lessonRepositoryMock.Verify(x => x.AddLesson(It.IsAny<Lesson>()), Times.Never);
    }

    [Fact]
    public async Task AdminAddLesson_WithException_ReturnsError()
    {
        // Arrange
        var dto = new AdminCreateLessonDto
        {
            Title = "Lesson 1",
            Description = "Test Description",
            CourseId = 1
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(dto.CourseId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _lessonService.AdminAddLesson(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("lỗi", result.Message);
    }

    #endregion

    #region TeacherAddLesson Tests

    [Fact]
    public async Task TeacherAddLesson_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var dto = new TeacherCreateLessonDto
        {
            Title = "Lesson 1",
            Description = "Test Description",
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Teacher Course",
            Type = CourseType.Teacher,
            Status = CourseStatus.Published,
            TeacherId = userId
        };

        var teacherPackage = new TeacherPackage
        {
            TeacherPackageId = 1,
            MaxLessons = 10
        };

        var lesson = new Lesson
        {
            LessonId = 1,
            Title = dto.Title,
            Description = dto.Description,
            CourseId = dto.CourseId
        };

        var lessonDto = new LessonDto
        {
            LessonId = 1,
            Title = dto.Title,
            Description = dto.Description,
            CourseId = dto.CourseId
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(dto.CourseId))
            .ReturnsAsync(course);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(userId))
            .ReturnsAsync(teacherPackage);

        _courseRepositoryMock
            .Setup(x => x.CountLessons(dto.CourseId))
            .ReturnsAsync(5); // Current lesson count

        _lessonRepositoryMock
            .Setup(x => x.LessonIncourse(dto.Title, dto.CourseId))
            .ReturnsAsync(false);

        _lessonRepositoryMock
            .Setup(x => x.AddLesson(It.IsAny<Lesson>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(x => x.Map<LessonDto>(It.IsAny<Lesson>()))
            .Returns(lessonDto);

        // Act
        var result = await _lessonService.TeacherAddLesson(dto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode); // Create operations return 201
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Title, result.Data.Title);

        _courseRepositoryMock.Verify(x => x.GetCourseById(dto.CourseId), Times.Once);
        _teacherPackageRepositoryMock.Verify(x => x.GetInformationTeacherpackage(userId), Times.Once);
    }

    [Fact]
    public async Task TeacherAddLesson_WithNonTeacherCourse_ReturnsForbidden()
    {
        // Arrange
        var userId = 1;
        var dto = new TeacherCreateLessonDto
        {
            Title = "Lesson 1",
            Description = "Test Description",
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "System Course",
            Type = CourseType.System,
            Status = CourseStatus.Published
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(dto.CourseId))
            .ReturnsAsync(course);

        // Act
        var result = await _lessonService.TeacherAddLesson(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Chỉ có thể thêm bài học vào khóa học của giáo viên", result.Message);

        _lessonRepositoryMock.Verify(x => x.AddLesson(It.IsAny<Lesson>()), Times.Never);
    }

    [Fact]
    public async Task TeacherAddLesson_WithWrongOwner_ReturnsForbidden()
    {
        // Arrange
        var userId = 1;
        var ownerId = 2;
        var dto = new TeacherCreateLessonDto
        {
            Title = "Lesson 1",
            Description = "Test Description",
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Teacher Course",
            Type = CourseType.Teacher,
            Status = CourseStatus.Published,
            TeacherId = ownerId // Different owner
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(dto.CourseId))
            .ReturnsAsync(course);

        // Act
        var result = await _lessonService.TeacherAddLesson(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Bạn không có quyền thêm bài học", result.Message);

        _lessonRepositoryMock.Verify(x => x.AddLesson(It.IsAny<Lesson>()), Times.Never);
    }

    [Fact]
    public async Task TeacherAddLesson_WithExceededPackageLimit_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var dto = new TeacherCreateLessonDto
        {
            Title = "Lesson 1",
            Description = "Test Description",
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Teacher Course",
            Type = CourseType.Teacher,
            Status = CourseStatus.Published,
            TeacherId = userId
        };

        var teacherPackage = new TeacherPackage
        {
            TeacherPackageId = 1,
            MaxLessons = 10
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(dto.CourseId))
            .ReturnsAsync(course);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(userId))
            .ReturnsAsync(teacherPackage);

        _courseRepositoryMock
            .Setup(x => x.CountLessons(dto.CourseId))
            .ReturnsAsync(10); // Already at limit

        // Act
        var result = await _lessonService.TeacherAddLesson(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode); // Returns 403, not 400
        Assert.Contains("Đã đạt số lượng bài học tối đa", result.Message); // Case sensitive

        _lessonRepositoryMock.Verify(x => x.AddLesson(It.IsAny<Lesson>()), Times.Never);
    }

    #endregion

    #region GetLessonById Tests

    [Fact]
    public async Task GetLessonById_WithValidId_ReturnsLesson()
    {
        // Arrange
        var lessonId = 1;
        var userId = 1;
        var userRole = "Student";

        var lesson = new Lesson
        {
            LessonId = lessonId,
            Title = "Test Lesson",
            Description = "Test Description",
            CourseId = 1,
            ImageKey = "lessons/real/image-123"
        };

        var lessonDto = new LessonDto
        {
            LessonId = lessonId,
            Title = "Test Lesson",
            Description = "Test Description",
            CourseId = 1
        };

        _lessonRepositoryMock
            .Setup(x => x.GetLessonById(lessonId))
            .ReturnsAsync(lesson);

        _mapperMock
            .Setup(x => x.Map<LessonDto>(lesson))
            .Returns(lessonDto);

        // Act
        var result = await _lessonService.GetLessonById(lessonId, userId, userRole);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode); // Get operations return 200
        Assert.NotNull(result.Data);
        Assert.Equal(lessonId, result.Data.LessonId);
        Assert.NotNull(result.Data.ImageUrl); // Should be built from ImageKey

        _lessonRepositoryMock.Verify(x => x.GetLessonById(lessonId), Times.Once);
    }

    [Fact]
    public async Task GetLessonById_WithNonExistentLesson_ReturnsNotFound()
    {
        // Arrange
        var lessonId = 999;
        var userId = 1;
        var userRole = "Student";

        _lessonRepositoryMock
            .Setup(x => x.GetLessonById(lessonId))
            .ReturnsAsync((Lesson?)null);

        // Act
        var result = await _lessonService.GetLessonById(lessonId, userId, userRole);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy bài học", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetLessonById_AsTeacherWithOwnCourse_ReturnsLesson()
    {
        // Arrange
        var lessonId = 1;
        var userId = 1;
        var userRole = "Teacher";

        var lesson = new Lesson
        {
            LessonId = lessonId,
            Title = "Test Lesson",
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Teacher Course",
            Type = CourseType.Teacher,
            TeacherId = userId
        };

        var lessonDto = new LessonDto
        {
            LessonId = lessonId,
            Title = "Test Lesson",
            CourseId = 1
        };

        _lessonRepositoryMock
            .Setup(x => x.GetLessonById(lessonId))
            .ReturnsAsync(lesson);

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(lesson.CourseId))
            .ReturnsAsync(course);

        _mapperMock
            .Setup(x => x.Map<LessonDto>(lesson))
            .Returns(lessonDto);

        // Act
        var result = await _lessonService.GetLessonById(lessonId, userId, userRole);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        _courseRepositoryMock.Verify(x => x.GetCourseById(lesson.CourseId), Times.Once);
    }

    [Fact]
    public async Task GetLessonById_AsTeacherWithWrongCourse_ReturnsForbidden()
    {
        // Arrange
        var lessonId = 1;
        var userId = 1;
        var ownerId = 2;
        var userRole = "Teacher";

        var lesson = new Lesson
        {
            LessonId = lessonId,
            Title = "Test Lesson",
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Teacher Course",
            Type = CourseType.Teacher,
            TeacherId = ownerId // Different owner
        };

        _lessonRepositoryMock
            .Setup(x => x.GetLessonById(lessonId))
            .ReturnsAsync(lesson);

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(lesson.CourseId))
            .ReturnsAsync(course);

        // Act
        var result = await _lessonService.GetLessonById(lessonId, userId, userRole);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Bạn không có quyền truy cập", result.Message);
    }

    [Fact]
    public async Task GetLessonById_WithNullImageKey_DoesNotBuildUrl()
    {
        // Arrange
        var lessonId = 1;
        var userId = 1;
        var userRole = "Student";

        var lesson = new Lesson
        {
            LessonId = lessonId,
            Title = "Test Lesson",
            CourseId = 1,
            ImageKey = null
        };

        var lessonDto = new LessonDto
        {
            LessonId = lessonId,
            Title = "Test Lesson"
        };

        _lessonRepositoryMock
            .Setup(x => x.GetLessonById(lessonId))
            .ReturnsAsync(lesson);

        _mapperMock
            .Setup(x => x.Map<LessonDto>(lesson))
            .Returns(lessonDto);

        // Act
        var result = await _lessonService.GetLessonById(lessonId, userId, userRole);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Null(result.Data.ImageUrl);
    }

    #endregion

    #region GetLessonsWithProgressByCourseIdAsync Tests

    [Fact]
    public async Task GetLessonsWithProgressByCourseIdAsync_WithValidCourseId_ReturnsLessonsWithProgress()
    {
        // Arrange
        var courseId = 1;
        var userId = 1;

        var lessons = new List<Lesson>
        {
            new Lesson
            {
                LessonId = 1,
                Title = "Lesson 1",
                CourseId = courseId,
                OrderIndex = 1
            },
            new Lesson
            {
                LessonId = 2,
                Title = "Lesson 2",
                CourseId = courseId,
                OrderIndex = 2
            }
        };

        var lessonCompletion = new LessonCompletion
        {
            LessonCompletionId = 1,
            UserId = userId,
            LessonId = 1,
            CompletionPercentage = 50,
            CompletedModules = 2,
            TotalModules = 4
        };

        var course = new Course
        {
            CourseId = courseId,
            Title = "Test Course",
            Type = CourseType.System
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(true);

        _lessonRepositoryMock
            .Setup(x => x.GetListLessonByCourseId(courseId))
            .ReturnsAsync(lessons);

        _lessonCompletionRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, 1))
            .ReturnsAsync(lessonCompletion);

        _lessonCompletionRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, 2))
            .ReturnsAsync((LessonCompletion?)null);

        // Act
        var result = await _lessonService.GetLessonsWithProgressByCourseIdAsync(courseId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode); // Get operations return 200
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);

        var lesson1 = result.Data.First(l => l.LessonId == 1);
        Assert.Equal(50, lesson1.CompletionPercentage);
        Assert.Equal(2, lesson1.CompletedModules);
        Assert.Equal(4, lesson1.TotalModules);

        var lesson2 = result.Data.First(l => l.LessonId == 2);
        Assert.Equal(0, lesson2.CompletionPercentage);
        Assert.Equal(0, lesson2.CompletedModules);

        _lessonRepositoryMock.Verify(x => x.GetListLessonByCourseId(courseId), Times.Once);
    }

    [Fact]
    public async Task GetLessonsWithProgressByCourseIdAsync_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var courseId = 1;
        var userId = 1;

        var course = new Course
        {
            CourseId = courseId,
            Title = "Test Course",
            Type = CourseType.System
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(true);

        _lessonRepositoryMock
            .Setup(x => x.GetListLessonByCourseId(courseId))
            .ReturnsAsync(new List<Lesson>());

        // Act
        var result = await _lessonService.GetLessonsWithProgressByCourseIdAsync(courseId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetLessonsWithProgressByCourseIdAsync_WithException_ReturnsError()
    {
        // Arrange
        var courseId = 1;
        var userId = 1;

        var course = new Course
        {
            CourseId = courseId,
            Title = "Test Course",
            Type = CourseType.System
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(true);

        _lessonRepositoryMock
            .Setup(x => x.GetListLessonByCourseId(courseId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _lessonService.GetLessonsWithProgressByCourseIdAsync(courseId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("lỗi", result.Message);
    }

    #endregion
}

