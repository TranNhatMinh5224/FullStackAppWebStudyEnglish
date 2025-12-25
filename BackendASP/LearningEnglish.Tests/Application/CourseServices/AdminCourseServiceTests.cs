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

public class AdminCourseServiceTests
{
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AdminCourseService>> _loggerMock;
    private readonly Mock<IMinioFileStorage> _minioFileStorageMock;
    private readonly AdminCourseService _adminCourseService;

    public AdminCourseServiceTests()
    {
        // Cấu hình BuildPublicUrl cho tests
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Minio:BaseUrl"]).Returns("http://localhost:9000");
        BuildPublicUrl.Configure(configMock.Object);

        _courseRepositoryMock = new Mock<ICourseRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AdminCourseService>>();
        _minioFileStorageMock = new Mock<IMinioFileStorage>();

        _adminCourseService = new AdminCourseService(
            _courseRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _minioFileStorageMock.Object
        );
    }

    #region GetAllCoursesPagedAsync Tests

    [Fact]
    public async Task GetAllCoursesPagedAsync_WithValidRequest_ReturnsPagedCourses()
    {
        // Arrange
        var request = new PageRequest { PageNumber = 1, PageSize = 10 };
        var courses = new List<Course>
        {
            new Course
            {
                CourseId = 1,
                Title = "Course 1",
                Type = CourseType.System,
                ImageKey = "course1.jpg",
                ImageType = "image/jpeg",
                Teacher = null
            },
            new Course
            {
                CourseId = 2,
                Title = "Course 2",
                Type = CourseType.System,
                ImageKey = "course2.jpg",
                ImageType = "image/jpeg",
                Teacher = null
            }
        };

        var pagedData = new PagedResult<Course>
        {
            Items = courses,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        var courseDtos = new List<AdminCourseListResponseDto>
        {
            new AdminCourseListResponseDto { CourseId = 1, Title = "Course 1" },
            new AdminCourseListResponseDto { CourseId = 2, Title = "Course 2" }
        };

        _courseRepositoryMock
            .Setup(x => x.GetAllCoursesPagedAsync(It.IsAny<CourseQueryParameters>()))
            .ReturnsAsync(pagedData);

        _mapperMock
            .Setup(x => x.Map<AdminCourseListResponseDto>(It.IsAny<Course>()))
            .Returns<Course>(c => courseDtos.First(dto => dto.CourseId == c.CourseId));

        _courseRepositoryMock
            .Setup(x => x.CountLessons(It.IsAny<int>()))
            .ReturnsAsync(5);

        _courseRepositoryMock
            .Setup(x => x.CountEnrolledUsers(It.IsAny<int>()))
            .ReturnsAsync(10);

        // Act
        var result = await _adminCourseService.GetAllCoursesPagedAsync(new CourseQueryParameters { PageNumber = request.PageNumber, PageSize = request.PageSize });

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Items.Count);
        Assert.Equal(2, result.Data.TotalCount);
        Assert.All(result.Data.Items, item => Assert.NotNull(item.ImageUrl));
    }

    [Fact]
    public async Task GetAllCoursesPagedAsync_WithEmptyList_ReturnsEmptyPagedResult()
    {
        // Arrange
        var request = new PageRequest { PageNumber = 1, PageSize = 10 };
        var pagedData = new PagedResult<Course>
        {
            Items = new List<Course>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _courseRepositoryMock
            .Setup(x => x.GetAllCoursesPagedAsync(It.IsAny<CourseQueryParameters>()))
            .ReturnsAsync(pagedData);

        // Act
        var result = await _adminCourseService.GetAllCoursesPagedAsync(new CourseQueryParameters { PageNumber = request.PageNumber, PageSize = request.PageSize });

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Items);
        Assert.Equal(0, result.Data.TotalCount);
    }

    [Fact]
    public async Task GetAllCoursesPagedAsync_WithCourseWithTeacher_ReturnsTeacherName()
    {
        // Arrange
        var request = new PageRequest { PageNumber = 1, PageSize = 10 };
        var teacher = new User
        {
            UserId = 1,
            FirstName = "John",
            LastName = "Doe"
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Course 1",
            Type = CourseType.Teacher,
            Teacher = teacher,
            TeacherId = 1
        };

        var pagedData = new PagedResult<Course>
        {
            Items = new List<Course> { course },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        var courseDto = new AdminCourseListResponseDto { CourseId = 1, Title = "Course 1" };

        _courseRepositoryMock
            .Setup(x => x.GetAllCoursesPagedAsync(It.IsAny<CourseQueryParameters>()))
            .ReturnsAsync(pagedData);

        _mapperMock
            .Setup(x => x.Map<AdminCourseListResponseDto>(course))
            .Returns(courseDto);

        _courseRepositoryMock
            .Setup(x => x.CountLessons(1))
            .ReturnsAsync(0);

        _courseRepositoryMock
            .Setup(x => x.CountEnrolledUsers(1))
            .ReturnsAsync(0);

        // Act
        var result = await _adminCourseService.GetAllCoursesPagedAsync(new CourseQueryParameters { PageNumber = request.PageNumber, PageSize = request.PageSize });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("John Doe", result.Data.Items.First().TeacherName);
    }

    [Fact]
    public async Task GetAllCoursesPagedAsync_WithException_ReturnsError()
    {
        // Arrange
        var request = new PageRequest { PageNumber = 1, PageSize = 10 };

        _courseRepositoryMock
            .Setup(x => x.GetAllCoursesPagedAsync(It.IsAny<CourseQueryParameters>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _adminCourseService.GetAllCoursesPagedAsync(new CourseQueryParameters { PageNumber = request.PageNumber, PageSize = request.PageSize });

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("lỗi", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region AdminCreateCourseAsync Tests

    [Fact]
    public async Task AdminCreateCourseAsync_WithValidRequest_ReturnsCreatedCourse()
    {
        // Arrange
        var requestDto = new AdminCreateCourseRequestDto
        {
            Title = "New Course",
            Description = "Course Description",
            Price = 100000,
            MaxStudent = 50,
            IsFeatured = true,
            Type = CourseType.System
        };

        var course = new Course
        {
            CourseId = 1,
            Title = requestDto.Title,
            DescriptionMarkdown = requestDto.Description,
            Price = requestDto.Price,
            MaxStudent = requestDto.MaxStudent,
            IsFeatured = requestDto.IsFeatured,
            Type = requestDto.Type,
            EnrollmentCount = 0
        };

        var courseDto = new CourseResponseDto
        {
            CourseId = 1,
            Title = requestDto.Title,
            Description = requestDto.Description
        };

        _courseRepositoryMock
            .Setup(x => x.AddCourse(It.IsAny<Course>()))
            .Returns(Task.CompletedTask)
            .Callback<Course>(c => c.CourseId = 1);

        _mapperMock
            .Setup(x => x.Map<CourseResponseDto>(It.IsAny<Course>()))
            .Returns(courseDto);

        // Act
        var result = await _adminCourseService.AdminCreateCourseAsync(requestDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("New Course", result.Data.Title);
        Assert.Equal("System Admin", result.Data.TeacherName);

        _courseRepositoryMock.Verify(x => x.AddCourse(It.IsAny<Course>()), Times.Once);
    }

    [Fact]
    public async Task AdminCreateCourseAsync_WithImageTempKey_CommitsImageAndCreatesCourse()
    {
        // Arrange
        var requestDto = new AdminCreateCourseRequestDto
        {
            Title = "New Course",
            Description = "Course Description",
            ImageTempKey = "temp-image-key",
            ImageType = "image/jpeg",
            Price = 0,
            MaxStudent = 50,
            Type = CourseType.System
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
        var result = await _adminCourseService.AdminCreateCourseAsync(requestDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        _minioFileStorageMock.Verify(x => x.CommitFileAsync("temp-image-key", "courses", "real"), Times.Once);
        _courseRepositoryMock.Verify(x => x.AddCourse(It.Is<Course>(c => c.ImageKey == "real-image-key")), Times.Once);
    }

    [Fact]
    public async Task AdminCreateCourseAsync_WithImageCommitFailure_ReturnsError()
    {
        // Arrange
        var requestDto = new AdminCreateCourseRequestDto
        {
            Title = "New Course",
            Description = "Course Description",
            ImageTempKey = "temp-image-key",
            ImageType = "image/jpeg",
            Price = 0,
            MaxStudent = 50,
            Type = CourseType.System
        };

        var commitResult = new ServiceResponse<string>
        {
            Success = false,
            Data = null
        };

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync("temp-image-key", "courses", "real"))
            .ReturnsAsync(commitResult);

        // Act
        var result = await _adminCourseService.AdminCreateCourseAsync(requestDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("ảnh", result.Message, StringComparison.OrdinalIgnoreCase);
        _courseRepositoryMock.Verify(x => x.AddCourse(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task AdminCreateCourseAsync_WithDatabaseError_RollsBackImage()
    {
        // Arrange
        var requestDto = new AdminCreateCourseRequestDto
        {
            Title = "New Course",
            Description = "Course Description",
            ImageTempKey = "temp-image-key",
            ImageType = "image/jpeg",
            Price = 0,
            MaxStudent = 50,
            Type = CourseType.System
        };

        var commitResult = new ServiceResponse<string>
        {
            Success = true,
            Data = "real-image-key"
        };

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
        var result = await _adminCourseService.AdminCreateCourseAsync(requestDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync("real-image-key", "courses"), Times.Once);
    }

    [Fact]
    public async Task AdminCreateCourseAsync_WithException_ReturnsError()
    {
        // Arrange
        var requestDto = new AdminCreateCourseRequestDto
        {
            Title = "New Course",
            Description = "Course Description",
            Price = 0,
            MaxStudent = 50,
            Type = CourseType.System
        };

        _courseRepositoryMock
            .Setup(x => x.AddCourse(It.IsAny<Course>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _adminCourseService.AdminCreateCourseAsync(requestDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
    }

    #endregion

    #region AdminUpdateCourseAsync Tests

    [Fact]
    public async Task AdminUpdateCourseAsync_WithValidRequest_ReturnsUpdatedCourse()
    {
        // Arrange
        var courseId = 1;
        var requestDto = new AdminUpdateCourseRequestDto
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Price = 200000
        };

        var existingCourse = new Course
        {
            CourseId = courseId,
            Title = "Original Title",
            DescriptionMarkdown = "Original Description",
            Price = 100000,
            MaxStudent = 50,
            Type = CourseType.System
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

        _courseRepositoryMock
            .Setup(x => x.UpdateCourse(It.IsAny<Course>()))
            .Returns(Task.CompletedTask);

        _courseRepositoryMock
            .Setup(x => x.CountLessons(courseId))
            .ReturnsAsync(5);

        _mapperMock
            .Setup(x => x.Map<CourseResponseDto>(It.IsAny<Course>()))
            .Returns(updatedCourseDto);

        // Act
        var result = await _adminCourseService.AdminUpdateCourseAsync(courseId, requestDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("Updated Title", result.Data.Title);
        _courseRepositoryMock.Verify(x => x.UpdateCourse(It.Is<Course>(c => c.Title == "Updated Title")), Times.Once);
    }

    [Fact]
    public async Task AdminUpdateCourseAsync_WithNonExistentCourse_ReturnsNotFound()
    {
        // Arrange
        var courseId = 999;
        var requestDto = new AdminUpdateCourseRequestDto
        {
            Title = "Updated Title"
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        // Act
        var result = await _adminCourseService.AdminUpdateCourseAsync(courseId, requestDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy", result.Message);
        _courseRepositoryMock.Verify(x => x.UpdateCourse(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task AdminUpdateCourseAsync_WithNewImage_CommitsNewImageAndDeletesOld()
    {
        // Arrange
        var courseId = 1;
        var requestDto = new AdminUpdateCourseRequestDto
        {
            Title = "Updated Title",
            ImageTempKey = "new-temp-image-key",
            ImageType = "image/png"
        };

        var existingCourse = new Course
        {
            CourseId = courseId,
            Title = "Original Title",
            ImageKey = "old-image-key",
            ImageType = "image/jpeg"
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

        _mapperMock
            .Setup(x => x.Map<CourseResponseDto>(It.IsAny<Course>()))
            .Returns(courseDto);

        // Act
        var result = await _adminCourseService.AdminUpdateCourseAsync(courseId, requestDto);

        // Assert
        Assert.True(result.Success);
        _minioFileStorageMock.Verify(x => x.CommitFileAsync("new-temp-image-key", "courses", "real"), Times.Once);
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync("old-image-key", "courses"), Times.Once);
        _courseRepositoryMock.Verify(x => x.UpdateCourse(It.Is<Course>(c => c.ImageKey == "new-real-image-key")), Times.Once);
    }

    [Fact]
    public async Task AdminUpdateCourseAsync_WithImageCommitFailure_ReturnsError()
    {
        // Arrange
        var courseId = 1;
        var requestDto = new AdminUpdateCourseRequestDto
        {
            Title = "Updated Title",
            ImageTempKey = "new-temp-image-key",
            ImageType = "image/png"
        };

        var existingCourse = new Course
        {
            CourseId = courseId,
            Title = "Original Title"
        };

        var commitResult = new ServiceResponse<string>
        {
            Success = false,
            Data = null
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync("new-temp-image-key", "courses", "real"))
            .ReturnsAsync(commitResult);

        // Act
        var result = await _adminCourseService.AdminUpdateCourseAsync(courseId, requestDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        _courseRepositoryMock.Verify(x => x.UpdateCourse(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task AdminUpdateCourseAsync_WithDatabaseError_RollsBackNewImage()
    {
        // Arrange
        var courseId = 1;
        var requestDto = new AdminUpdateCourseRequestDto
        {
            Title = "Updated Title",
            ImageTempKey = "new-temp-image-key",
            ImageType = "image/png"
        };

        var existingCourse = new Course
        {
            CourseId = courseId,
            Title = "Original Title"
        };

        var commitResult = new ServiceResponse<string>
        {
            Success = true,
            Data = "new-real-image-key"
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync("new-temp-image-key", "courses", "real"))
            .ReturnsAsync(commitResult);

        _courseRepositoryMock
            .Setup(x => x.UpdateCourse(It.IsAny<Course>()))
            .ThrowsAsync(new Exception("Database error"));

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync("new-real-image-key", "courses"))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true });

        // Act
        var result = await _adminCourseService.AdminUpdateCourseAsync(courseId, requestDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync("new-real-image-key", "courses"), Times.Once);
    }

    [Fact]
    public async Task AdminUpdateCourseAsync_WithPartialUpdate_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var courseId = 1;
        var requestDto = new AdminUpdateCourseRequestDto
        {
            Title = "Updated Title"
            // Only title, other fields are null
        };

        var existingCourse = new Course
        {
            CourseId = courseId,
            Title = "Original Title",
            DescriptionMarkdown = "Original Description",
            Price = 100000,
            MaxStudent = 50
        };

        var courseDto = new CourseResponseDto { CourseId = courseId, Title = "Updated Title" };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);

        _courseRepositoryMock
            .Setup(x => x.UpdateCourse(It.IsAny<Course>()))
            .Returns(Task.CompletedTask);

        _courseRepositoryMock
            .Setup(x => x.CountLessons(courseId))
            .ReturnsAsync(0);

        _mapperMock
            .Setup(x => x.Map<CourseResponseDto>(It.IsAny<Course>()))
            .Returns(courseDto);

        // Act
        var result = await _adminCourseService.AdminUpdateCourseAsync(courseId, requestDto);

        // Assert
        Assert.True(result.Success);
        _courseRepositoryMock.Verify(x => x.UpdateCourse(It.Is<Course>(c => 
            c.Title == "Updated Title" && 
            c.DescriptionMarkdown == "Original Description" &&
            c.Price == 100000)), Times.Once);
    }

    [Fact]
    public async Task AdminUpdateCourseAsync_WithException_ReturnsError()
    {
        // Arrange
        var courseId = 1;
        var requestDto = new AdminUpdateCourseRequestDto
        {
            Title = "Updated Title"
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _adminCourseService.AdminUpdateCourseAsync(courseId, requestDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
    }

    #endregion

    #region DeleteCourseAsync Tests

    [Fact]
    public async Task DeleteCourseAsync_WithValidCourseId_ReturnsSuccess()
    {
        // Arrange
        var courseId = 1;
        var course = new Course
        {
            CourseId = courseId,
            Title = "Course to Delete",
            ImageKey = "image-key.jpg"
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync("image-key.jpg", "courses"))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true });

        _courseRepositoryMock
            .Setup(x => x.DeleteCourse(courseId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _adminCourseService.DeleteCourseAsync(courseId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);
        Assert.Contains("Xóa", result.Message);
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync("image-key.jpg", "courses"), Times.Once);
        _courseRepositoryMock.Verify(x => x.DeleteCourse(courseId), Times.Once);
    }

    [Fact]
    public async Task DeleteCourseAsync_WithNonExistentCourse_ReturnsNotFound()
    {
        // Arrange
        var courseId = 999;

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        // Act
        var result = await _adminCourseService.DeleteCourseAsync(courseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy", result.Message);
        _courseRepositoryMock.Verify(x => x.DeleteCourse(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCourseAsync_WithCourseWithoutImage_DeletesCourseOnly()
    {
        // Arrange
        var courseId = 1;
        var course = new Course
        {
            CourseId = courseId,
            Title = "Course to Delete",
            ImageKey = null
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.DeleteCourse(courseId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _adminCourseService.DeleteCourseAsync(courseId);

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
        var course = new Course
        {
            CourseId = courseId,
            Title = "Course to Delete",
            ImageKey = "image-key.jpg"
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync("image-key.jpg", "courses"))
            .ThrowsAsync(new Exception("Image delete failed"));

        _courseRepositoryMock
            .Setup(x => x.DeleteCourse(courseId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _adminCourseService.DeleteCourseAsync(courseId);

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

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _adminCourseService.DeleteCourseAsync(courseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
    }

    #endregion
}

