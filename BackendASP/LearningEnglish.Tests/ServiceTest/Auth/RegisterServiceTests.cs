using AutoMapper;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Auth;
using LearningEnglish.Application.Service;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LearningEnglish.Tests.ServiceTest.Auth;

public class RegisterServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IEmailVerificationTokenRepository> _emailTokenRepoMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly RegisterService _service;

    public RegisterServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _emailTokenRepoMock = new Mock<IEmailVerificationTokenRepository>();
        _emailSenderMock = new Mock<IEmailSender>();
        _mapperMock = new Mock<IMapper>();

        _service = new RegisterService(
            _userRepositoryMock.Object,
            _emailTokenRepoMock.Object,
            _emailSenderMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task RegisterUserAsync_EmailExistsAndVerified_ReturnsBadRequest()
    {
        // Arrange
        var dto = new RegisterUserDto { Email = "exists@test.com" };
        var existingUser = new User { Email = "exists@test.com", EmailVerified = true };
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync(existingUser);

        // Act
        var result = await _service.RegisterUserAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Email đã tồn tại trong hệ thống", result.Message);
    }

    [Fact]
    public async Task RegisterUserAsync_PhoneExistsAndVerified_ReturnsBadRequest()
    {
        // Arrange
        var dto = new RegisterUserDto { Email = "new@test.com", PhoneNumber = "1234567890" };
        var existingPhoneUser = new User { PhoneNumber = "1234567890", EmailVerified = true };
        
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync((User)null);
        _userRepositoryMock.Setup(r => r.GetUserByPhoneNumberAsync("1234567890")).ReturnsAsync(existingPhoneUser);

        // Act
        var result = await _service.RegisterUserAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Số điện thoại đã tồn tại trong hệ thống", result.Message);
    }

    [Fact]
    public async Task RegisterUserAsync_Success_CreatesUserAndSendsOtp()
    {
        // Arrange
        var dto = new RegisterUserDto { Email = "new@test.com", Password = "Pass123!", FirstName = "Test", LastName = "User" };
        var user = new User { UserId = 1, Email = dto.Email, Roles = new List<Role>() };

        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync((User)null);
        _mapperMock.Setup(m => m.Map<User>(dto)).Returns(user);
        _userRepositoryMock.Setup(r => r.GetRoleByNameAsync("Student")).ReturnsAsync(new Role { Name = "Student" });
        _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(new UserDto { Email = dto.Email });

        // Act
        var result = await _service.RegisterUserAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        _userRepositoryMock.Verify(r => r.AddUserAsync(user), Times.Once);
        _emailTokenRepoMock.Verify(r => r.AddAsync(It.IsAny<EmailVerificationToken>()), Times.Once);
        _emailSenderMock.Verify(s => s.SendEmailAsync(dto.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_TokenNotFound_ReturnsBadRequest()
    {
        // Arrange
        var dto = new VerifyEmailDto { Email = "test@test.com", OtpCode = "123456" };
        _emailTokenRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((EmailVerificationToken)null);

        // Act
        var result = await _service.VerifyEmailAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Mã OTP không hợp lệ", result.Message);
    }

    [Fact]
    public async Task VerifyEmailAsync_TokenExpired_ReturnsBadRequest()
    {
        // Arrange
        var dto = new VerifyEmailDto { Email = "test@test.com", OtpCode = "123456" };
        var token = new EmailVerificationToken { ExpiresAt = DateTime.UtcNow.AddMinutes(-10) }; // Expired
        _emailTokenRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(token);

        // Act
        var result = await _service.VerifyEmailAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Mã OTP đã hết hạn", result.Message);
        _emailTokenRepoMock.Verify(r => r.DeleteAsync(token), Times.Once); // Should delete expired token
    }

    [Fact]
    public async Task VerifyEmailAsync_Success_VerifiesUser()
    {
        // Arrange
        var dto = new VerifyEmailDto { Email = "test@test.com", OtpCode = "123456" };
        var token = new EmailVerificationToken 
        { 
            OtpCode = "123456", 
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            AttemptsCount = 0 
        };
        var user = new User { Email = "test@test.com", EmailVerified = false };

        _emailTokenRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(token);
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(dto.Email)).ReturnsAsync(user);

        // Act
        var result = await _service.VerifyEmailAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.True(user.EmailVerified);
        _emailTokenRepoMock.Verify(r => r.DeleteAsync(token), Times.Once); // Clean up used token
    }
}
