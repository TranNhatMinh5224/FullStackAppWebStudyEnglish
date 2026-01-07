using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.Auth;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.Service;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.Extensions.Configuration;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type

namespace LearningEnglish.Tests.ServiceTest.Users;

public class InformationUserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMinioFileStorage> _minioFileStorageMock;
    private readonly Mock<ILogger<InformationUserService>> _loggerMock;
    private readonly Mock<IStreakService> _streakServiceMock;
    private readonly Mock<ITeacherSubscriptionRepository> _teacherSubscriptionRepositoryMock;
    private readonly InformationUserService _service;

    public InformationUserServiceTests()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Minio:BaseUrl"]).Returns("https://example.com");
        BuildPublicUrl.Configure(configMock.Object);

        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _minioFileStorageMock = new Mock<IMinioFileStorage>();
        _loggerMock = new Mock<ILogger<InformationUserService>>();
        _streakServiceMock = new Mock<IStreakService>();
        _teacherSubscriptionRepositoryMock = new Mock<ITeacherSubscriptionRepository>();

        _service = new InformationUserService(
            _userRepositoryMock.Object,
            _mapperMock.Object,
            _minioFileStorageMock.Object,
            _loggerMock.Object,
            _streakServiceMock.Object,
            _teacherSubscriptionRepositoryMock.Object
        );
    }

    [Fact]
    public async Task GetUserProfileAsync_UserExists_ReturnsUserDto()
    {
        // Arrange
        var userId = 1;
        var user = new User { UserId = userId, FirstName = "John", LastName = "Doe", AvatarKey = "avatar123" };
        var userDto = new UserDto { UserId = userId, FirstName = "John", LastName = "Doe" };
        var streakDto = new StreakDto { CurrentStreak = 5 };
        var subscription = new TeacherSubscription { TeacherSubscriptionId = 1 };
        var subscriptionDto = new UserTeacherSubscriptionDto { IsTeacher = true, PackageLevel = "Premium" };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(userDto);
        _streakServiceMock.Setup(s => s.GetCurrentStreakAsync(userId)).ReturnsAsync(new LearningEnglish.Application.Common.ServiceResponse<StreakDto> { Success = true, Data = streakDto });
        _teacherSubscriptionRepositoryMock.Setup(t => t.GetActiveSubscriptionAsync(userId)).ReturnsAsync(subscription);
        _mapperMock.Setup(m => m.Map<UserTeacherSubscriptionDto>(subscription)).Returns(subscriptionDto);

        // Act
        var result = await _service.GetUserProfileAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(userDto, result.Data);
        Assert.Equal("https://example.com/avatars/avatar123", result.Data.AvatarUrl);
        Assert.Equal(streakDto, result.Data.Streak);
        Assert.Equal(subscriptionDto, result.Data.TeacherSubscription);
    }

    [Fact]
    public async Task GetUserProfileAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = 1;
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null);

        // Act
        var result = await _service.GetUserProfileAsync(userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Không tìm thấy người dùng", result.Message);
    }

    [Fact]
    public async Task GetUserProfileAsync_ExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var userId = 1;
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.GetUserProfileAsync(userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Equal("Đã xảy ra lỗi hệ thống", result.Message);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_UserExists_UpdatesSuccessfully()
    {
        // Arrange
        var userId = 1;
        var dto = new UpdateUserDto { FirstName = "Jane", PhoneNumber = "123456789" };
        var user = new User { UserId = userId, FirstName = "John", PhoneNumber = "987654321" };
        var updatedUserDto = new UserDto { UserId = userId, FirstName = "Jane", PhoneNumber = "123456789" };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.GetUserByPhoneNumberAsync(dto.PhoneNumber)).ReturnsAsync((User)null);
        _mapperMock.Setup(m => m.Map(dto, user));
        _userRepositoryMock.Setup(r => r.UpdateUserAsync(user)).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(updatedUserDto);

        // Act
        var result = await _service.UpdateUserProfileAsync(userId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Message);
        Assert.Equal("Cập nhật hồ sơ thành công", result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(updatedUserDto, result.Data);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = 1;
        var dto = new UpdateUserDto();
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null);

        // Act
        var result = await _service.UpdateUserProfileAsync(userId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.NotNull(result.Message);
        Assert.Equal("Không tìm thấy người dùng", result.Message);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_PhoneNumberExists_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var dto = new UpdateUserDto { PhoneNumber = "123456789" };
        var user = new User { UserId = userId, PhoneNumber = "987654321" };
        var existingUser = new User { UserId = 2, PhoneNumber = "123456789" };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.GetUserByPhoneNumberAsync(dto.PhoneNumber)).ReturnsAsync(existingUser);

        // Act
        var result = await _service.UpdateUserProfileAsync(userId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.NotNull(result.Message);
        Assert.Equal("Số điện thoại đã tồn tại trong hệ thống", result.Message);
    }

    [Fact]
    public async Task UpdateAvatarAsync_UserExists_UpdatesSuccessfully()
    {
        // Arrange
        var userId = 1;
        var dto = new UpdateAvatarDto { AvatarTempKey = "temp123" };
        var user = new User { UserId = userId, AvatarKey = "oldAvatar" };
        var commitResult = new LearningEnglish.Application.Common.ServiceResponse<string> { Success = true, Data = "committed123" };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _minioFileStorageMock.Setup(m => m.CommitFileAsync("temp123", "avatars", "real")).ReturnsAsync(commitResult);
        _minioFileStorageMock.Setup(m => m.DeleteFileAsync("oldAvatar", "avatars")).ReturnsAsync(new LearningEnglish.Application.Common.ServiceResponse<bool> { Success = true, Data = true });
        _userRepositoryMock.Setup(r => r.UpdateUserAsync(user)).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAvatarAsync(userId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Message);
        Assert.Equal("Cập nhật avatar thành công", result.Message);
        Assert.True(result.Data);
        Assert.Equal("committed123", user.AvatarKey);
    }

    [Fact]
    public async Task UpdateAvatarAsync_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = 1;
        var dto = new UpdateAvatarDto();
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null);

        // Act
        var result = await _service.UpdateAvatarAsync(userId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.NotNull(result.Message);
        Assert.Equal("Không tìm thấy người dùng", result.Message);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task UpdateAvatarAsync_CommitFails_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var dto = new UpdateAvatarDto { AvatarTempKey = "temp123" };
        var user = new User { UserId = userId };
        var commitResult = new LearningEnglish.Application.Common.ServiceResponse<string> { Success = false };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _minioFileStorageMock.Setup(m => m.CommitFileAsync("temp123", "avatars", "real")).ReturnsAsync(commitResult);

        // Act
        var result = await _service.UpdateAvatarAsync(userId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.NotNull(result.Message);
        Assert.Equal("Không thể lưu avatar. Vui lòng thử lại.", result.Message);
        Assert.False(result.Data);
    }
}