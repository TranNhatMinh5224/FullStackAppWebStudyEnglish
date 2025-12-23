using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using AutoMapper;
using Moq;
using Microsoft.Extensions.Configuration;

namespace LearningEnglish.Tests.Application;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<IMinioFileStorage> _minioFileStorageMock;
    private readonly Mock<IStreakService> _streakServiceMock;
    private readonly Mock<ITeacherSubscriptionRepository> _teacherSubscriptionRepositoryMock;
    private readonly Mock<ICourseProgressRepository> _courseProgressRepositoryMock;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<UserManagementService>> _loggerMock;
    private readonly UserManagementService _userManagementService;

    public UserServiceTests()
    {
        // Cấu hình BuildPublicUrl cho tests
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Minio:BaseUrl"]).Returns("http://localhost:9000");
        BuildPublicUrl.Configure(configMock.Object);

        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _minioFileStorageMock = new Mock<IMinioFileStorage>();
        _streakServiceMock = new Mock<IStreakService>();
        _teacherSubscriptionRepositoryMock = new Mock<ITeacherSubscriptionRepository>();
        _courseProgressRepositoryMock = new Mock<ICourseProgressRepository>();
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<UserManagementService>>();

        _userManagementService = new UserManagementService(
            _userRepositoryMock.Object,
            _mapperMock.Object,
            _courseRepositoryMock.Object,
            _minioFileStorageMock.Object,
            _streakServiceMock.Object,
            _teacherSubscriptionRepositoryMock.Object,
            _courseProgressRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    #region GetUserProfileAsync Tests

    [Fact]
    public async Task GetUserProfileAsync_WithValidUserId_ReturnsUserDto()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            UserId = userId,
            FirstName = "Minh",
            LastName = "Tran",
            Email = "minh@example.com"
        };

        var userDto = new UserDto
        {
            UserId = userId,
            FirstName = "Minh",
            LastName = "Tran",
            Email = "minh@example.com"
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _mapperMock.Setup(x => x.Map<UserDto>(user)).Returns(userDto);
        _streakServiceMock.Setup(x => x.GetCurrentStreakAsync(userId))
            .ReturnsAsync(new ServiceResponse<StreakDto> { Success = false });
        _teacherSubscriptionRepositoryMock.Setup(x => x.GetActiveSubscriptionAsync(userId))
            .ReturnsAsync((TeacherSubscription?)null);

        // Act
        var result = await _userManagementService.GetUserProfileAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Minh", result.Data.FirstName);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public async Task GetUserProfileAsync_WithInvalidUserId_ReturnsNotFound()
    {
        // Arrange
        var userId = 999;

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act
        var result = await _userManagementService.GetUserProfileAsync(userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Không tìm thấy người dùng", result.Message);
    }

    [Fact]
    public async Task GetUserProfileAsync_WithAvatar_ReturnsUserDtoWithAvatarUrl()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            UserId = userId,
            FirstName = "Minh",
            LastName = "Tran",
            Email = "minh@example.com",
            AvatarKey = "avatars/real/avatar-123"
        };

        var userDto = new UserDto
        {
            UserId = userId,
            FirstName = "Minh",
            LastName = "Tran",
            Email = "minh@example.com"
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _mapperMock.Setup(x => x.Map<UserDto>(user)).Returns(userDto);
        _streakServiceMock.Setup(x => x.GetCurrentStreakAsync(userId))
            .ReturnsAsync(new ServiceResponse<StreakDto> { Success = false });
        _teacherSubscriptionRepositoryMock.Setup(x => x.GetActiveSubscriptionAsync(userId))
            .ReturnsAsync((TeacherSubscription?)null);

        // Act
        var result = await _userManagementService.GetUserProfileAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.AvatarUrl);
    }

    [Fact]
    public async Task GetUserProfileAsync_WithStreak_ReturnsUserDtoWithStreak()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            UserId = userId,
            FirstName = "Minh",
            LastName = "Tran",
            Email = "minh@example.com"
        };

        var userDto = new UserDto
        {
            UserId = userId,
            FirstName = "Minh",
            LastName = "Tran",
            Email = "minh@example.com"
        };

        var streakDto = new StreakDto
        {
            CurrentStreak = 5
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _mapperMock.Setup(x => x.Map<UserDto>(user)).Returns(userDto);
        _streakServiceMock.Setup(x => x.GetCurrentStreakAsync(userId))
            .ReturnsAsync(new ServiceResponse<StreakDto> { Success = true, Data = streakDto });
        _teacherSubscriptionRepositoryMock.Setup(x => x.GetActiveSubscriptionAsync(userId))
            .ReturnsAsync((TeacherSubscription?)null);

        // Act
        var result = await _userManagementService.GetUserProfileAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Streak);
        Assert.Equal(5, result.Data.Streak.CurrentStreak);
    }

    #endregion

    #region UpdateUserProfileAsync Tests

    [Fact]
    public async Task UpdateUserProfileAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var dto = new UpdateUserDto
        {
            FirstName = "Updated",
            LastName = "Name",
            PhoneNumber = "0123456789",
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        var user = new User
        {
            UserId = userId,
            FirstName = "Original",
            LastName = "Name",
            PhoneNumber = "0987654321"
        };

        var updatedUser = new User
        {
            UserId = userId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            DateOfBirth = dto.DateOfBirth
        };

        var userDto = new UserDto
        {
            UserId = userId,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.GetUserByPhoneNumberAsync(dto.PhoneNumber))
            .ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _mapperMock.Setup(x => x.Map(It.IsAny<UpdateUserDto>(), It.IsAny<User>()))
            .Callback<UpdateUserDto, User>((d, u) =>
            {
                u.FirstName = d.FirstName;
                u.LastName = d.LastName;
                u.PhoneNumber = d.PhoneNumber;
                u.DateOfBirth = d.DateOfBirth;
            });
        _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>())).Returns(userDto);

        // Act
        var result = await _userManagementService.UpdateUserProfileAsync(userId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Contains("Cập nhật hồ sơ thành công", result.Message);
        Assert.NotNull(result.Data);

        _userRepositoryMock.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = 999;
        var dto = new UpdateUserDto
        {
            FirstName = "Updated",
            LastName = "Name"
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act
        var result = await _userManagementService.UpdateUserProfileAsync(userId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Không tìm thấy người dùng", result.Message);

        _userRepositoryMock.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_WithDuplicatePhoneNumber_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var otherUserId = 2;
        var dto = new UpdateUserDto
        {
            FirstName = "Updated",
            LastName = "Name",
            PhoneNumber = "0123456789"
        };

        var user = new User
        {
            UserId = userId,
            FirstName = "Original",
            LastName = "Name",
            PhoneNumber = "0987654321"
        };

        var existingUser = new User
        {
            UserId = otherUserId,
            PhoneNumber = dto.PhoneNumber
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.GetUserByPhoneNumberAsync(dto.PhoneNumber))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userManagementService.UpdateUserProfileAsync(userId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Số điện thoại đã tồn tại", result.Message);

        _userRepositoryMock.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_WithSamePhoneNumber_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var phoneNumber = "0123456789";
        var dto = new UpdateUserDto
        {
            FirstName = "Updated",
            LastName = "Name",
            PhoneNumber = phoneNumber
        };

        var user = new User
        {
            UserId = userId,
            FirstName = "Original",
            LastName = "Name",
            PhoneNumber = phoneNumber
        };

        var userDto = new UserDto
        {
            UserId = userId,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _mapperMock.Setup(x => x.Map(It.IsAny<UpdateUserDto>(), It.IsAny<User>()))
            .Callback<UpdateUserDto, User>((d, u) =>
            {
                u.FirstName = d.FirstName;
                u.LastName = d.LastName;
            });
        _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>())).Returns(userDto);

        // Act
        var result = await _userManagementService.UpdateUserProfileAsync(userId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);

        // Should not check phone number if it's the same
        _userRepositoryMock.Verify(x => x.GetUserByPhoneNumberAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region UpdateAvatarAsync Tests

    [Fact]
    public async Task UpdateAvatarAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var dto = new UpdateAvatarDto
        {
            AvatarTempKey = "temp/avatar-123"
        };

        var user = new User
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User",
            AvatarKey = null
        };

        var committedKey = "avatars/real/avatar-123";

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _minioFileStorageMock.Setup(x => x.CommitFileAsync(dto.AvatarTempKey, "avatars", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = committedKey
            });
        _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _userManagementService.UpdateAvatarAsync(userId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);
        Assert.Contains("Cập nhật avatar thành công", result.Message);

        _minioFileStorageMock.Verify(x => x.CommitFileAsync(dto.AvatarTempKey, "avatars", "real"), Times.Once);
        _userRepositoryMock.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAvatarAsync_WithExistingAvatar_DeletesOldAvatar()
    {
        // Arrange
        var userId = 1;
        var dto = new UpdateAvatarDto
        {
            AvatarTempKey = "temp/avatar-123"
        };

        var oldAvatarKey = "avatars/real/old-avatar";
        var user = new User
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User",
            AvatarKey = oldAvatarKey
        };

        var committedKey = "avatars/real/avatar-123";

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _minioFileStorageMock.Setup(x => x.CommitFileAsync(dto.AvatarTempKey, "avatars", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = committedKey
            });
        _minioFileStorageMock.Setup(x => x.DeleteFileAsync(oldAvatarKey, "avatars"))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true, Data = true });
        _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _userManagementService.UpdateAvatarAsync(userId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);

        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(oldAvatarKey, "avatars"), Times.Once);
    }

    [Fact]
    public async Task UpdateAvatarAsync_WithCommitFailure_ReturnsError()
    {
        // Arrange
        var userId = 1;
        var dto = new UpdateAvatarDto
        {
            AvatarTempKey = "temp/avatar-123"
        };

        var user = new User
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User"
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _minioFileStorageMock.Setup(x => x.CommitFileAsync(dto.AvatarTempKey, "avatars", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = false,
                Message = "Failed to commit"
            });

        // Act
        var result = await _userManagementService.UpdateAvatarAsync(userId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.False(result.Data);
        Assert.Contains("Không thể lưu avatar", result.Message);

        _userRepositoryMock.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAvatarAsync_WithDatabaseError_RollsBackFile()
    {
        // Arrange
        var userId = 1;
        var dto = new UpdateAvatarDto
        {
            AvatarTempKey = "temp/avatar-123"
        };

        var user = new User
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User"
        };

        var committedKey = "avatars/real/avatar-123";

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _minioFileStorageMock.Setup(x => x.CommitFileAsync(dto.AvatarTempKey, "avatars", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = committedKey
            });
        _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("Database error"));
        _minioFileStorageMock.Setup(x => x.DeleteFileAsync(committedKey, "avatars"))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true, Data = true });

        // Act
        var result = await _userManagementService.UpdateAvatarAsync(userId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.False(result.Data);

        // Should rollback by deleting the committed file
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(committedKey, "avatars"), Times.Once);
    }

    [Fact]
    public async Task UpdateAvatarAsync_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = 999;
        var dto = new UpdateAvatarDto
        {
            AvatarTempKey = "temp/avatar-123"
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act
        var result = await _userManagementService.UpdateAvatarAsync(userId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.False(result.Data);
        Assert.Equal("Không tìm thấy người dùng", result.Message);
    }

    #endregion

    #region BlockAccountAsync Tests

    [Fact]
    public async Task BlockAccountAsync_WithValidUser_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User",
            Status = AccountStatus.Active
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.GetUserRolesAsync(userId)).ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _userManagementService.BlockAccountAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Contains("Block tài khoản thành công", result.Data.Message);

        _userRepositoryMock.Verify(x => x.UpdateUserAsync(It.Is<User>(u => u.Status == AccountStatus.Inactive)), Times.Once);
    }

    [Fact]
    public async Task BlockAccountAsync_WithAdmin_ReturnsForbidden()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            UserId = userId,
            FirstName = "Admin",
            LastName = "User",
            Status = AccountStatus.Active
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.GetUserRolesAsync(userId)).ReturnsAsync(true);

        // Act
        var result = await _userManagementService.BlockAccountAsync(userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Không thể block tài khoản Admin", result.Message);

        _userRepositoryMock.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task BlockAccountAsync_WithAlreadyBlocked_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User",
            Status = AccountStatus.Inactive
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.GetUserRolesAsync(userId)).ReturnsAsync(false);

        // Act
        var result = await _userManagementService.BlockAccountAsync(userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Tài khoản đã bị khóa trước đó", result.Message);

        _userRepositoryMock.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task BlockAccountAsync_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = 999;

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act
        var result = await _userManagementService.BlockAccountAsync(userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy tài khoản người dùng", result.Message);
    }

    #endregion

    #region UnblockAccountAsync Tests

    [Fact]
    public async Task UnblockAccountAsync_WithBlockedUser_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User",
            Status = AccountStatus.Inactive
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _userManagementService.UnblockAccountAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Contains("Unblock tài khoản thành công", result.Data.Message);

        _userRepositoryMock.Verify(x => x.UpdateUserAsync(It.Is<User>(u => u.Status == AccountStatus.Active)), Times.Once);
    }

    [Fact]
    public async Task UnblockAccountAsync_WithActiveUser_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User",
            Status = AccountStatus.Active
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _userManagementService.UnblockAccountAsync(userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Tài khoản hiện không bị khóa", result.Message);

        _userRepositoryMock.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UnblockAccountAsync_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = 999;

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act
        var result = await _userManagementService.UnblockAccountAsync(userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy tài khoản người dùng", result.Message);
    }

    #endregion
}
