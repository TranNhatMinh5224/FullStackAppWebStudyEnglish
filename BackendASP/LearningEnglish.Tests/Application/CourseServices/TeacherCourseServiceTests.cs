using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Moq;
using Microsoft.Extensions.Logging;
using AutoMapper;
using LearningEnglish.Application.Common.Helpers;
using Microsoft.Extensions.Configuration;

namespace LearningEnglish.Tests.Application.CourseServices;

public class TeacherCourseServiceTests
{
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<TeacherCourseService>> _loggerMock;
    private readonly Mock<ITeacherPackageRepository> _teacherPackageRepositoryMock;
    private readonly Mock<IMinioFileStorage> _minioFileStorageMock;
    private readonly TeacherCourseService _teacherCourseService;

    public TeacherCourseServiceTests()
    {
        // Cấu hình BuildPublicUrl cho tests
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Minio:BaseUrl"]).Returns("http://localhost:9000");
        BuildPublicUrl.Configure(configMock.Object);

        _courseRepositoryMock = new Mock<ICourseRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<TeacherCourseService>>();
        _teacherPackageRepositoryMock = new Mock<ITeacherPackageRepository>();
        _minioFileStorageMock = new Mock<IMinioFileStorage>();

        _teacherCourseService = new TeacherCourseService(
            _courseRepositoryMock.Object,
            _userRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _teacherPackageRepositoryMock.Object,
            _minioFileStorageMock.Object
        );
    }

    #region CreateCourseAsync Tests

    [Fact]
    public async Task CreateCourseAsync_WithValidRequest_ReturnsCreatedCourse()
    {
        // Arrange
        var teacherId = 1;
        var requestDto = new TeacherCreateCourseRequestDto
        {
            Title = "New Course",
            Description = "Course Description",
            MaxStudent = 50,
            Type = CourseType.Teacher
        };

        var teacher = new User
        {
            UserId = teacherId,
            FirstName = "John",
            LastName = "Doe"
        };

        var teacherPackage = new TeacherPackage
        {
            TeacherPackageId = 1,
            MaxCourses = 10,
            MaxStudents = 100
        };

        var course = new Course
        {
            CourseId = 1,
            Title = requestDto.Title,
            DescriptionMarkdown = requestDto.Description,
            TeacherId = teacherId,
            MaxStudent = requestDto.MaxStudent,
            Type = CourseType.Teacher,
            EnrollmentCount = 0
        };

        var courseDto = new CourseResponseDto
        {
            CourseId = 1,
            Title = requestDto.Title,
            Description = requestDto.Description
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(teacherId))
            .ReturnsAsync(teacher);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync(teacherPackage);

        _courseRepositoryMock
            .Setup(x => x.GetCoursesByTeacher(teacherId))
            .ReturnsAsync(new List<Course>());

        _courseRepositoryMock
            .Setup(x => x.AddCourse(It.IsAny<Course>()))
            .Returns(Task.CompletedTask)
            .Callback<Course>(c => c.CourseId = 1);

        _mapperMock
            .Setup(x => x.Map<CourseResponseDto>(It.IsAny<Course>()))
            .Returns(courseDto);

        // Act
        var result = await _teacherCourseService.CreateCourseAsync(requestDto, teacherId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("New Course", result.Data.Title);
        Assert.Contains("1/10", result.Message);
        _courseRepositoryMock.Verify(x => x.AddCourse(It.Is<Course>(c => c.TeacherId == teacherId)), Times.Once);
    }

    [Fact]
    public async Task CreateCourseAsync_WithNonExistentTeacher_ReturnsError()
    {
        // Arrange
        var teacherId = 999;
        var requestDto = new TeacherCreateCourseRequestDto
        {
            Title = "New Course",
            Description = "Course Description",
            MaxStudent = 50,
            Type = CourseType.Teacher
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(teacherId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _teacherCourseService.CreateCourseAsync(requestDto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Teacher not found", result.Message);
        _courseRepositoryMock.Verify(x => x.AddCourse(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task CreateCourseAsync_WithNoActivePackage_ReturnsError()
    {
        // Arrange
        var teacherId = 1;
        var requestDto = new TeacherCreateCourseRequestDto
        {
            Title = "New Course",
            Description = "Course Description",
            MaxStudent = 50,
            Type = CourseType.Teacher
        };

        var teacher = new User
        {
            UserId = teacherId
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(teacherId))
            .ReturnsAsync(teacher);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync((TeacherPackage?)null);

        // Act
        var result = await _teacherCourseService.CreateCourseAsync(requestDto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("subscription", result.Message, StringComparison.OrdinalIgnoreCase);
        _courseRepositoryMock.Verify(x => x.AddCourse(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task CreateCourseAsync_WithMaxCoursesReached_ReturnsError()
    {
        // Arrange
        var teacherId = 1;
        var requestDto = new TeacherCreateCourseRequestDto
        {
            Title = "New Course",
            Description = "Course Description",
            MaxStudent = 50,
            Type = CourseType.Teacher
        };

        var teacher = new User
        {
            UserId = teacherId
        };

        var teacherPackage = new TeacherPackage
        {
            TeacherPackageId = 1,
            MaxCourses = 5,
            MaxStudents = 100
        };

        var existingCourses = new List<Course>
        {
            new Course { CourseId = 1, TeacherId = teacherId },
            new Course { CourseId = 2, TeacherId = teacherId },
            new Course { CourseId = 3, TeacherId = teacherId },
            new Course { CourseId = 4, TeacherId = teacherId },
            new Course { CourseId = 5, TeacherId = teacherId }
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(teacherId))
            .ReturnsAsync(teacher);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync(teacherPackage);

        _courseRepositoryMock
            .Setup(x => x.GetCoursesByTeacher(teacherId))
            .ReturnsAsync(existingCourses);

        // Act
        var result = await _teacherCourseService.CreateCourseAsync(requestDto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("maximum", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("5/5", result.Message);
        _courseRepositoryMock.Verify(x => x.AddCourse(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task CreateCourseAsync_WithMaxStudentExceedingPackageLimit_ReturnsError()
    {
        // Arrange
        var teacherId = 1;
        var requestDto = new TeacherCreateCourseRequestDto
        {
            Title = "New Course",
            Description = "Course Description",
            MaxStudent = 150, // Exceeds package limit
            Type = CourseType.Teacher
        };

        var teacher = new User
        {
            UserId = teacherId
        };

        var teacherPackage = new TeacherPackage
        {
            TeacherPackageId = 1,
            MaxCourses = 10,
            MaxStudents = 100 // Package limit
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(teacherId))
            .ReturnsAsync(teacher);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync(teacherPackage);

        _courseRepositoryMock
            .Setup(x => x.GetCoursesByTeacher(teacherId))
            .ReturnsAsync(new List<Course>());

        // Act
        var result = await _teacherCourseService.CreateCourseAsync(requestDto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("MaxStudent", result.Message);
        Assert.Contains("100", result.Message);
        _courseRepositoryMock.Verify(x => x.AddCourse(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task CreateCourseAsync_WithZeroMaxStudent_UsesPackageMaxStudents()
    {
        // Arrange
        var teacherId = 1;
        var requestDto = new TeacherCreateCourseRequestDto
        {
            Title = "New Course",
            Description = "Course Description",
            MaxStudent = 0, // Will use package limit
            Type = CourseType.Teacher
        };

        var teacher = new User
        {
            UserId = teacherId
        };

        var teacherPackage = new TeacherPackage
        {
            TeacherPackageId = 1,
            MaxCourses = 10,
            MaxStudents = 100
        };

        var courseDto = new CourseResponseDto
        {
            CourseId = 1,
            Title = requestDto.Title
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(teacherId))
            .ReturnsAsync(teacher);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync(teacherPackage);

        _courseRepositoryMock
            .Setup(x => x.GetCoursesByTeacher(teacherId))
            .ReturnsAsync(new List<Course>());

        _courseRepositoryMock
            .Setup(x => x.AddCourse(It.IsAny<Course>()))
            .Returns(Task.CompletedTask)
            .Callback<Course>(c => c.CourseId = 1);

        _mapperMock
            .Setup(x => x.Map<CourseResponseDto>(It.IsAny<Course>()))
            .Returns(courseDto);

        // Act
        var result = await _teacherCourseService.CreateCourseAsync(requestDto, teacherId);

        // Assert
        Assert.True(result.Success);
        _courseRepositoryMock.Verify(x => x.AddCourse(It.Is<Course>(c => c.MaxStudent == 100)), Times.Once);
    }

    [Fact]
    public async Task CreateCourseAsync_WithImageTempKey_CommitsImageAndCreatesCourse()
    {
        // Arrange
        var teacherId = 1;
        var requestDto = new TeacherCreateCourseRequestDto
        {
            Title = "New Course",
            Description = "Course Description",
            ImageTempKey = "temp-image-key",
            ImageType = "image/jpeg",
            MaxStudent = 50,
            Type = CourseType.Teacher
        };

        var teacher = new User
        {
            UserId = teacherId
        };

        var teacherPackage = new TeacherPackage
        {
            TeacherPackageId = 1,
            MaxCourses = 10,
            MaxStudents = 100
        };

        var commitResult = new ServiceResponse<string>
        {
            Success = true,
            Data = "real-image-key"
        };

        var courseDto = new CourseResponseDto
        {
            CourseId = 1,
            Title = requestDto.Title
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(teacherId))
            .ReturnsAsync(teacher);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync(teacherPackage);

        _courseRepositoryMock
            .Setup(x => x.GetCoursesByTeacher(teacherId))
            .ReturnsAsync(new List<Course>());

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync("temp-image-key", "courses", "real"))
            .ReturnsAsync(commitResult);

        _courseRepositoryMock
            .Setup(x => x.AddCourse(It.IsAny<Course>()))
            .Returns(Task.CompletedTask)
            .Callback<Course>(c => c.CourseId = 1);

        _mapperMock
            .Setup(x => x.Map<CourseResponseDto>(It.IsAny<Course>()))
            .Returns(courseDto);

        // Act
        var result = await _teacherCourseService.CreateCourseAsync(requestDto, teacherId);

        // Assert
        Assert.True(result.Success);
        _minioFileStorageMock.Verify(x => x.CommitFileAsync("temp-image-key", "courses", "real"), Times.Once);
        _courseRepositoryMock.Verify(x => x.AddCourse(It.Is<Course>(c => c.ImageKey == "real-image-key")), Times.Once);
    }

    [Fact]
    public async Task CreateCourseAsync_WithImageCommitFailure_ReturnsError()
    {
        // Arrange
        var teacherId = 1;
        var requestDto = new TeacherCreateCourseRequestDto
        {
            Title = "New Course",
            Description = "Course Description",
            ImageTempKey = "temp-image-key",
            ImageType = "image/jpeg",
            MaxStudent = 50,
            Type = CourseType.Teacher
        };

        var teacher = new User
        {
            UserId = teacherId
        };

        var teacherPackage = new TeacherPackage
        {
            TeacherPackageId = 1,
            MaxCourses = 10,
            MaxStudents = 100
        };

        var commitResult = new ServiceResponse<string>
        {
            Success = false,
            Data = null
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(teacherId))
            .ReturnsAsync(teacher);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync(teacherPackage);

        _courseRepositoryMock
            .Setup(x => x.GetCoursesByTeacher(teacherId))
            .ReturnsAsync(new List<Course>());

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync("temp-image-key", "courses", "real"))
            .ReturnsAsync(commitResult);

        // Act
        var result = await _teacherCourseService.CreateCourseAsync(requestDto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("ảnh", result.Message, StringComparison.OrdinalIgnoreCase);
        _courseRepositoryMock.Verify(x => x.AddCourse(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task CreateCourseAsync_WithDatabaseError_RollsBackImage()
    {
        // Arrange
        var teacherId = 1;
        var requestDto = new TeacherCreateCourseRequestDto
        {
            Title = "New Course",
            Description = "Course Description",
            ImageTempKey = "temp-image-key",
            ImageType = "image/jpeg",
            MaxStudent = 50,
            Type = CourseType.Teacher
        };

        var teacher = new User
        {
            UserId = teacherId
        };

        var teacherPackage = new TeacherPackage
        {
            TeacherPackageId = 1,
            MaxCourses = 10,
            MaxStudents = 100
        };

        var commitResult = new ServiceResponse<string>
        {
            Success = true,
            Data = "real-image-key"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(teacherId))
            .ReturnsAsync(teacher);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync(teacherPackage);

        _courseRepositoryMock
            .Setup(x => x.GetCoursesByTeacher(teacherId))
            .ReturnsAsync(new List<Course>());

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync("temp-image-key", "courses", "real"))
            .ReturnsAsync(commitResult);

        _courseRepositoryMock
            .Setup(x => x.AddCourse(It.IsAny<Course>()))
            .ThrowsAsync(new Exception("Database error"));

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync("real-image-key", "courses"))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true });

        // Act
        var result = await _teacherCourseService.CreateCourseAsync(requestDto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync("real-image-key", "courses"), Times.Once);
    }

    [Fact]
    public async Task CreateCourseAsync_WithException_ReturnsError()
    {
        // Arrange
        var teacherId = 1;
        var requestDto = new TeacherCreateCourseRequestDto
        {
            Title = "New Course",
            Description = "Course Description",
            MaxStudent = 50,
            Type = CourseType.Teacher
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(teacherId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _teacherCourseService.CreateCourseAsync(requestDto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Error", result.Message);
    }

    #endregion

    #region UpdateCourseAsync Tests

    [Fact]
    public async Task UpdateCourseAsync_WithValidRequest_ReturnsUpdatedCourse()
    {
        // Arrange
        var courseId = 1;
        var teacherId = 1;
        var requestDto = new TeacherUpdateCourseRequestDto
        {
            Title = "Updated Title",
            Description = "Updated Description",
            MaxStudent = 60,
            Type = CourseType.Teacher
        };

        var existingCourse = new Course
        {
            CourseId = courseId,
            Title = "Original Title",
            DescriptionMarkdown = "Original Description",
            TeacherId = teacherId,
            MaxStudent = 50,
            Type = CourseType.Teacher,
            EnrollmentCount = 10
        };

        var teacherPackage = new TeacherPackage
        {
            TeacherPackageId = 1,
            MaxCourses = 10,
            MaxStudents = 100
        };

        var updatedCourseDto = new CourseResponseDto
        {
            CourseId = courseId,
            Title = "Updated Title",
            Description = "Updated Description"
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync(teacherPackage);

        _courseRepositoryMock
            .Setup(x => x.UpdateCourse(It.IsAny<Course>()))
            .Returns(Task.CompletedTask);

        _courseRepositoryMock
            .Setup(x => x.CountLessons(courseId))
            .ReturnsAsync(5);

        _courseRepositoryMock
            .Setup(x => x.CountEnrolledUsers(courseId))
            .ReturnsAsync(10);

        _mapperMock
            .Setup(x => x.Map<CourseResponseDto>(It.IsAny<Course>()))
            .Returns(updatedCourseDto);

        // Act
        var result = await _teacherCourseService.UpdateCourseAsync(courseId, requestDto, teacherId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("Updated Title", result.Data.Title);
        _courseRepositoryMock.Verify(x => x.UpdateCourse(It.Is<Course>(c => c.Title == "Updated Title")), Times.Once);
    }

    [Fact]
    public async Task UpdateCourseAsync_WithNonExistentCourse_ReturnsNotFound()
    {
        // Arrange
        var courseId = 999;
        var teacherId = 1;
        var requestDto = new TeacherUpdateCourseRequestDto
        {
            Title = "Updated Title"
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        // Act
        var result = await _teacherCourseService.UpdateCourseAsync(courseId, requestDto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy", result.Message);
        _courseRepositoryMock.Verify(x => x.UpdateCourse(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCourseAsync_WithDifferentOwner_ReturnsForbidden()
    {
        // Arrange
        var courseId = 1;
        var teacherId = 1;
        var ownerId = 2; // Different owner
        var requestDto = new TeacherUpdateCourseRequestDto
        {
            Title = "Updated Title"
        };

        var existingCourse = new Course
        {
            CourseId = courseId,
            Title = "Original Title",
            TeacherId = ownerId
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);

        // Act
        var result = await _teacherCourseService.UpdateCourseAsync(courseId, requestDto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("quyền", result.Message, StringComparison.OrdinalIgnoreCase);
        _courseRepositoryMock.Verify(x => x.UpdateCourse(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCourseAsync_WithMaxStudentExceedingPackageLimit_ReturnsError()
    {
        // Arrange
        var courseId = 1;
        var teacherId = 1;
        var requestDto = new TeacherUpdateCourseRequestDto
        {
            Title = "Updated Title",
            MaxStudent = 150 // Exceeds package limit
        };

        var existingCourse = new Course
        {
            CourseId = courseId,
            Title = "Original Title",
            TeacherId = teacherId,
            EnrollmentCount = 10
        };

        var teacherPackage = new TeacherPackage
        {
            TeacherPackageId = 1,
            MaxCourses = 10,
            MaxStudents = 100 // Package limit
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync(teacherPackage);

        // Act
        var result = await _teacherCourseService.UpdateCourseAsync(courseId, requestDto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Số học sinh tối đa", result.Message);
        Assert.Contains("100", result.Message);
        _courseRepositoryMock.Verify(x => x.UpdateCourse(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCourseAsync_WithMaxStudentBelowEnrollmentCount_ReturnsError()
    {
        // Arrange
        var courseId = 1;
        var teacherId = 1;
        var requestDto = new TeacherUpdateCourseRequestDto
        {
            Title = "Updated Title",
            MaxStudent = 5 // Below enrollment count
        };

        var existingCourse = new Course
        {
            CourseId = courseId,
            Title = "Original Title",
            TeacherId = teacherId,
            EnrollmentCount = 10 // Already has 10 students
        };

        var teacherPackage = new TeacherPackage
        {
            TeacherPackageId = 1,
            MaxCourses = 10,
            MaxStudents = 100
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync(teacherPackage);

        // Act
        var result = await _teacherCourseService.UpdateCourseAsync(courseId, requestDto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("thấp hơn", result.Message);
        Assert.Contains("10", result.Message);
        _courseRepositoryMock.Verify(x => x.UpdateCourse(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCourseAsync_WithZeroMaxStudent_UsesPackageMaxStudents()
    {
        // Arrange
        var courseId = 1;
        var teacherId = 1;
        var requestDto = new TeacherUpdateCourseRequestDto
        {
            Title = "Updated Title",
            MaxStudent = 0 // Will use package limit
        };

        var existingCourse = new Course
        {
            CourseId = courseId,
            Title = "Original Title",
            TeacherId = teacherId,
            EnrollmentCount = 10
        };

        var teacherPackage = new TeacherPackage
        {
            TeacherPackageId = 1,
            MaxCourses = 10,
            MaxStudents = 100
        };

        var courseDto = new CourseResponseDto
        {
            CourseId = courseId,
            Title = "Updated Title"
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync(teacherPackage);

        _courseRepositoryMock
            .Setup(x => x.UpdateCourse(It.IsAny<Course>()))
            .Returns(Task.CompletedTask);

        _courseRepositoryMock
            .Setup(x => x.CountLessons(courseId))
            .ReturnsAsync(0);

        _courseRepositoryMock
            .Setup(x => x.CountEnrolledUsers(courseId))
            .ReturnsAsync(10);

        _mapperMock
            .Setup(x => x.Map<CourseResponseDto>(It.IsAny<Course>()))
            .Returns(courseDto);

        // Act
        var result = await _teacherCourseService.UpdateCourseAsync(courseId, requestDto, teacherId);

        // Assert
        Assert.True(result.Success);
        _courseRepositoryMock.Verify(x => x.UpdateCourse(It.Is<Course>(c => c.MaxStudent == 100)), Times.Once);
    }

    [Fact]
    public async Task UpdateCourseAsync_WithNewImage_CommitsNewImageAndDeletesOld()
    {
        // Arrange
        var courseId = 1;
        var teacherId = 1;
        var requestDto = new TeacherUpdateCourseRequestDto
        {
            Title = "Updated Title",
            ImageTempKey = "new-temp-image-key",
            ImageType = "image/png"
        };

        var existingCourse = new Course
        {
            CourseId = courseId,
            Title = "Original Title",
            TeacherId = teacherId,
            ImageKey = "old-image-key",
            ImageType = "image/jpeg",
            EnrollmentCount = 0
        };

        var teacherPackage = new TeacherPackage
        {
            TeacherPackageId = 1,
            MaxCourses = 10,
            MaxStudents = 100
        };

        var commitResult = new ServiceResponse<string>
        {
            Success = true,
            Data = "new-real-image-key"
        };

        var courseDto = new CourseResponseDto { CourseId = courseId, Title = "Updated Title" };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync(teacherPackage);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync("new-temp-image-key", "courses", "real"))
            .ReturnsAsync(commitResult);

        _courseRepositoryMock
            .Setup(x => x.UpdateCourse(It.IsAny<Course>()))
            .Returns(Task.CompletedTask);

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync("old-image-key", "courses"))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true });

        _courseRepositoryMock
            .Setup(x => x.CountLessons(courseId))
            .ReturnsAsync(0);

        _courseRepositoryMock
            .Setup(x => x.CountEnrolledUsers(courseId))
            .ReturnsAsync(0);

        _mapperMock
            .Setup(x => x.Map<CourseResponseDto>(It.IsAny<Course>()))
            .Returns(courseDto);

        // Act
        var result = await _teacherCourseService.UpdateCourseAsync(courseId, requestDto, teacherId);

        // Assert
        Assert.True(result.Success);
        _minioFileStorageMock.Verify(x => x.CommitFileAsync("new-temp-image-key", "courses", "real"), Times.Once);
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync("old-image-key", "courses"), Times.Once);
        _courseRepositoryMock.Verify(x => x.UpdateCourse(It.Is<Course>(c => c.ImageKey == "new-real-image-key")), Times.Once);
    }

    [Fact]
    public async Task UpdateCourseAsync_WithNoActivePackage_ReturnsError()
    {
        // Arrange
        var courseId = 1;
        var teacherId = 1;
        var requestDto = new TeacherUpdateCourseRequestDto
        {
            Title = "Updated Title",
            MaxStudent = 50
        };

        var existingCourse = new Course
        {
            CourseId = courseId,
            Title = "Original Title",
            TeacherId = teacherId,
            EnrollmentCount = 0
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync((TeacherPackage?)null);

        // Act
        var result = await _teacherCourseService.UpdateCourseAsync(courseId, requestDto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("gói đăng ký", result.Message);
        _courseRepositoryMock.Verify(x => x.UpdateCourse(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCourseAsync_WithException_ReturnsError()
    {
        // Arrange
        var courseId = 1;
        var teacherId = 1;
        var requestDto = new TeacherUpdateCourseRequestDto
        {
            Title = "Updated Title"
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _teacherCourseService.UpdateCourseAsync(courseId, requestDto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
    }

    #endregion

    #region GetMyCoursesPagedAsync Tests

    [Fact]
    public async Task GetMyCoursesPagedAsync_WithValidRequest_ReturnsPagedCourses()
    {
        // Arrange
        var teacherId = 1;
        var request = new PageRequest { PageNumber = 1, PageSize = 10 };
        var courses = new List<Course>
        {
            new Course
            {
                CourseId = 1,
                Title = "Course 1",
                TeacherId = teacherId,
                Type = CourseType.Teacher,
                ImageKey = "course1.jpg"
            },
            new Course
            {
                CourseId = 2,
                Title = "Course 2",
                TeacherId = teacherId,
                Type = CourseType.Teacher,
                ImageKey = "course2.jpg"
            }
        };

        var pagedData = new PagedResult<Course>
        {
            Items = courses,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        var courseDtos = new List<CourseResponseDto>
        {
            new CourseResponseDto { CourseId = 1, Title = "Course 1" },
            new CourseResponseDto { CourseId = 2, Title = "Course 2" }
        };

        _courseRepositoryMock
            .Setup(x => x.GetCoursesByTeacherPagedAsync(teacherId, request))
            .ReturnsAsync(pagedData);

        _mapperMock
            .Setup(x => x.Map<CourseResponseDto>(It.IsAny<Course>()))
            .Returns<Course>(c => courseDtos.First(dto => dto.CourseId == c.CourseId));

        _courseRepositoryMock
            .Setup(x => x.CountLessons(It.IsAny<int>()))
            .ReturnsAsync(5);

        _courseRepositoryMock
            .Setup(x => x.CountEnrolledUsers(It.IsAny<int>()))
            .ReturnsAsync(10);

        // Act
        var result = await _teacherCourseService.GetMyCoursesPagedAsync(teacherId, request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Items.Count);
        Assert.Equal(2, result.Data.TotalCount);
        Assert.All(result.Data.Items, item => Assert.NotNull(item.ImageUrl));
    }

    [Fact]
    public async Task GetMyCoursesPagedAsync_WithEmptyList_ReturnsEmptyPagedResult()
    {
        // Arrange
        var teacherId = 1;
        var request = new PageRequest { PageNumber = 1, PageSize = 10 };
        var pagedData = new PagedResult<Course>
        {
            Items = new List<Course>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _courseRepositoryMock
            .Setup(x => x.GetCoursesByTeacherPagedAsync(teacherId, request))
            .ReturnsAsync(pagedData);

        // Act
        var result = await _teacherCourseService.GetMyCoursesPagedAsync(teacherId, request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Items);
        Assert.Equal(0, result.Data.TotalCount);
    }

    [Fact]
    public async Task GetMyCoursesPagedAsync_WithException_ReturnsError()
    {
        // Arrange
        var teacherId = 1;
        var request = new PageRequest { PageNumber = 1, PageSize = 10 };

        _courseRepositoryMock
            .Setup(x => x.GetCoursesByTeacherPagedAsync(teacherId, request))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _teacherCourseService.GetMyCoursesPagedAsync(teacherId, request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Error", result.Message);
    }

    #endregion

    #region DeleteCourseAsync Tests

    [Fact]
    public async Task DeleteCourseAsync_WithValidCourseId_ReturnsSuccess()
    {
        // Arrange
        var courseId = 1;
        var teacherId = 1;
        var course = new Course
        {
            CourseId = courseId,
            Title = "Course to Delete",
            TeacherId = teacherId,
            ImageKey = "image-key.jpg"
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.DeleteCourse(courseId))
            .Returns(Task.CompletedTask);

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync("image-key.jpg", "courses"))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true });

        // Act
        var result = await _teacherCourseService.DeleteCourseAsync(courseId, teacherId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Contains("deleted successfully", result.Message);
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync("image-key.jpg", "courses"), Times.Once);
        _courseRepositoryMock.Verify(x => x.DeleteCourse(courseId), Times.Once);
    }

    [Fact]
    public async Task DeleteCourseAsync_WithNonExistentCourse_ReturnsNotFound()
    {
        // Arrange
        var courseId = 999;
        var teacherId = 1;

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        // Act
        var result = await _teacherCourseService.DeleteCourseAsync(courseId, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
        _courseRepositoryMock.Verify(x => x.DeleteCourse(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCourseAsync_WithDifferentOwner_ReturnsForbidden()
    {
        // Arrange
        var courseId = 1;
        var teacherId = 1;
        var ownerId = 2; // Different owner
        var course = new Course
        {
            CourseId = courseId,
            Title = "Course to Delete",
            TeacherId = ownerId
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        // Act
        var result = await _teacherCourseService.DeleteCourseAsync(courseId, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("quyền", result.Message, StringComparison.OrdinalIgnoreCase);
        _courseRepositoryMock.Verify(x => x.DeleteCourse(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCourseAsync_WithCourseWithoutImage_DeletesCourseOnly()
    {
        // Arrange
        var courseId = 1;
        var teacherId = 1;
        var course = new Course
        {
            CourseId = courseId,
            Title = "Course to Delete",
            TeacherId = teacherId,
            ImageKey = null
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.DeleteCourse(courseId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _teacherCourseService.DeleteCourseAsync(courseId, teacherId);

        // Assert
        Assert.True(result.Success);
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _courseRepositoryMock.Verify(x => x.DeleteCourse(courseId), Times.Once);
    }

    [Fact]
    public async Task DeleteCourseAsync_WithImageDeleteFailure_StillDeletesCourse()
    {
        // Arrange
        var courseId = 1;
        var teacherId = 1;
        var course = new Course
        {
            CourseId = courseId,
            Title = "Course to Delete",
            TeacherId = teacherId,
            ImageKey = "image-key.jpg"
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.DeleteCourse(courseId))
            .Returns(Task.CompletedTask);

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync("image-key.jpg", "courses"))
            .ThrowsAsync(new Exception("Image delete failed"));

        // Act
        var result = await _teacherCourseService.DeleteCourseAsync(courseId, teacherId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        // Course should still be deleted even if image deletion fails
        _courseRepositoryMock.Verify(x => x.DeleteCourse(courseId), Times.Once);
    }

    [Fact]
    public async Task DeleteCourseAsync_WithException_ReturnsError()
    {
        // Arrange
        var courseId = 1;
        var teacherId = 1;

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _teacherCourseService.DeleteCourseAsync(courseId, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
    }

    #endregion

    #region GetCourseDetailAsync Tests

    [Fact]
    public async Task GetCourseDetailAsync_WithValidCourseId_ReturnsCourseDetail()
    {
        // Arrange
        var courseId = 1;
        var teacherId = 1;
        var course = new Course
        {
            CourseId = courseId,
            Title = "Course Detail",
            DescriptionMarkdown = "Course Description",
            TeacherId = teacherId,
            Type = CourseType.Teacher
        };

        var courseDetailDto = new TeacherCourseDetailDto
        {
            CourseId = courseId,
            Title = "Course Detail",
            Description = "Course Description"
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync(course);

        _mapperMock
            .Setup(x => x.Map<TeacherCourseDetailDto>(course))
            .Returns(courseDetailDto);

        // Act
        var result = await _teacherCourseService.GetCourseDetailAsync(courseId, teacherId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("Course Detail", result.Data.Title);
    }

    [Fact]
    public async Task GetCourseDetailAsync_WithNonExistentCourse_ReturnsNotFound()
    {
        // Arrange
        var courseId = 999;
        var teacherId = 1;

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync((Course?)null);

        // Act
        var result = await _teacherCourseService.GetCourseDetailAsync(courseId, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetCourseDetailAsync_WithDifferentOwner_ReturnsForbidden()
    {
        // Arrange
        var courseId = 1;
        var teacherId = 1;
        var ownerId = 2; // Different owner
        var course = new Course
        {
            CourseId = courseId,
            Title = "Course Detail",
            TeacherId = ownerId
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync(course);

        // Act
        var result = await _teacherCourseService.GetCourseDetailAsync(courseId, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("permission", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetCourseDetailAsync_WithException_ReturnsError()
    {
        // Arrange
        var courseId = 1;
        var teacherId = 1;

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _teacherCourseService.GetCourseDetailAsync(courseId, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
    }

    #endregion
}

