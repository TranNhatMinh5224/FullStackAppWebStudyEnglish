using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using AutoMapper;
using Moq;

namespace LearningEnglish.Tests.Application;

public class UserServiceTests
{
    [Fact]
    public async Task GetUserProfileAsync_WithValidUserId_ReturnsUserDto()
    {
        // Arrange - Chuẩn bị mock data
        var mockUserRepo = new Mock<IUserRepository>();
        var mockMapper = new Mock<IMapper>();
        var mockCourseRepo = new Mock<ICourseRepository>();
        var mockMinioStorage = new Mock<IMinioFileStorage>();
        var mockStreakService = new Mock<IStreakService>();
        var mockTeacherSubRepo = new Mock<ITeacherSubscriptionRepository>();
        var mockCourseProgressRepo = new Mock<ICourseProgressRepository>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<UserManagementService>>();

        var user = new User
        {
            UserId = 1,
            FirstName = "Minh",
            LastName = "Nguyen",
            Email = "minh@example.com"
        };

        var userDto = new UserDto
        {
            UserId = 1,
            FirstName = "Minh",
            LastName = "Nguyen",
            Email = "minh@example.com"
        };

        mockUserRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        mockMapper.Setup(x => x.Map<UserDto>(user)).Returns(userDto);

        var service = new UserManagementService(
            mockUserRepo.Object,
            mockMapper.Object,
            mockCourseRepo.Object,
            mockMinioStorage.Object,
            mockStreakService.Object,
            mockTeacherSubRepo.Object,
            mockCourseProgressRepo.Object,
            mockLogger.Object
        );

        // Act - Gọi method cần test
        var result = await service.GetUserProfileAsync(1);

        // Assert - Kiểm tra kết quả
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Minh", result.Data.FirstName);
    }

    [Fact]
    public async Task GetUserProfileAsync_WithInvalidUserId_ReturnsNotFound()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();
        var mockMapper = new Mock<IMapper>();
        var mockCourseRepo = new Mock<ICourseRepository>();
        var mockMinioStorage = new Mock<IMinioFileStorage>();
        var mockStreakService = new Mock<IStreakService>();
        var mockTeacherSubRepo = new Mock<ITeacherSubscriptionRepository>();
        var mockCourseProgressRepo = new Mock<ICourseProgressRepository>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<UserManagementService>>();

        mockUserRepo.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((User?)null);

        var service = new UserManagementService(
            mockUserRepo.Object,
            mockMapper.Object,
            mockCourseRepo.Object,
            mockMinioStorage.Object,
            mockStreakService.Object,
            mockTeacherSubRepo.Object,
            mockCourseProgressRepo.Object,
            mockLogger.Object
        );

        // Act
        var result = await service.GetUserProfileAsync(999);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Không tìm thấy người dùng", result.Message);
    }
}
