using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Users;

public class UserTests
{
    [Fact]
    public void User_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        Assert.Equal(0, user.UserId);
        Assert.Equal(string.Empty, user.FirstName);
        Assert.Equal(string.Empty, user.LastName);
        Assert.Equal(string.Empty, user.Email);
        Assert.Null(user.DateOfBirth);
        Assert.True(user.IsMale);
        Assert.Equal(string.Empty, user.DisplayName);
        Assert.Equal(string.Empty, user.FullName);
        Assert.Equal(string.Empty, user.NormalizedEmail);
        Assert.False(user.EmailVerified);
        Assert.Null(user.PasswordHash);
        Assert.Equal(string.Empty, user.PhoneNumber);
        Assert.Null(user.AvatarKey);
        Assert.Equal(AccountStatus.Active, user.Status);
        Assert.Null(user.CurrentTeacherSubscriptionId);
        Assert.NotNull(user.Roles);
        Assert.Empty(user.Roles);
        Assert.NotNull(user.CreatedCourses);
        Assert.Empty(user.CreatedCourses);
        // Add more assertions for other collections if needed
    }

    [Fact]
    public void DisplayName_ShouldReturnTrimmedFullName()
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var displayName = user.DisplayName;

        // Assert
        Assert.Equal("John Doe", displayName);
    }

    [Fact]
    public void DisplayName_ShouldHandleEmptyNames()
    {
        // Arrange
        var user = new User();

        // Act
        var displayName = user.DisplayName;

        // Assert
        Assert.Equal(string.Empty, displayName);
    }

    [Fact]
    public void FullName_ShouldReturnTrimmedFullName()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Jane",
            LastName = "Smith"
        };

        // Act
        var fullName = user.FullName;

        // Assert
        Assert.Equal("Jane Smith", fullName);
    }

    [Fact]
    public void SetPassword_ShouldHashPassword()
    {
        // Arrange
        var user = new User();
        var password = "testpassword";

        // Act
        user.SetPassword(password);

        // Assert
        Assert.NotNull(user.PasswordHash);
        Assert.NotEqual(password, user.PasswordHash);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
    {
        // Arrange
        var user = new User();
        var password = "correctpassword";
        user.SetPassword(password);

        // Act
        var isValid = user.VerifyPassword(password);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
    {
        // Arrange
        var user = new User();
        user.SetPassword("correctpassword");

        // Act
        var isValid = user.VerifyPassword("wrongpassword");

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseWhenPasswordHashIsNull()
    {
        // Arrange
        var user = new User();

        // Act
        var isValid = user.VerifyPassword("anypassword");

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void HasLocalPassword_ShouldReturnTrueWhenPasswordHashIsSet()
    {
        // Arrange
        var user = new User();
        user.SetPassword("password");

        // Act
        var hasPassword = user.HasLocalPassword();

        // Assert
        Assert.True(hasPassword);
    }

    [Fact]
    public void HasLocalPassword_ShouldReturnFalseWhenPasswordHashIsEmpty()
    {
        // Arrange
        var user = new User();

        // Act
        var hasPassword = user.HasLocalPassword();

        // Assert
        Assert.False(hasPassword);
    }

    [Fact]
    public void HasExternalLogin_ShouldReturnTrueWhenProviderExists()
    {
        // Arrange
        var user = new User();
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Google" });

        // Act
        var hasLogin = user.HasExternalLogin("Google");

        // Assert
        Assert.True(hasLogin);
    }

    [Fact]
    public void HasExternalLogin_ShouldReturnFalseWhenProviderDoesNotExist()
    {
        // Arrange
        var user = new User();
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Google" });

        // Act
        var hasLogin = user.HasExternalLogin("Facebook");

        // Assert
        Assert.False(hasLogin);
    }

    [Fact]
    public void GetExternalLogin_ShouldReturnLoginWhenProviderExists()
    {
        // Arrange
        var user = new User();
        var login = new ExternalLogin { Provider = "Google" };
        user.ExternalLogins.Add(login);

        // Act
        var result = user.GetExternalLogin("Google");

        // Assert
        Assert.Equal(login, result);
    }

    [Fact]
    public void GetExternalLogin_ShouldReturnNullWhenProviderDoesNotExist()
    {
        // Arrange
        var user = new User();

        // Act
        var result = user.GetExternalLogin("Google");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void IsExternalUserOnly_ShouldReturnTrueWhenNoLocalPasswordAndHasExternalLogins()
    {
        // Arrange
        var user = new User();
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Google" });

        // Act
        var isExternalOnly = user.IsExternalUserOnly();

        // Assert
        Assert.True(isExternalOnly);
    }

    [Fact]
    public void IsExternalUserOnly_ShouldReturnFalseWhenHasLocalPassword()
    {
        // Arrange
        var user = new User();
        user.SetPassword("password");
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Google" });

        // Act
        var isExternalOnly = user.IsExternalUserOnly();

        // Assert
        Assert.False(isExternalOnly);
    }

    [Fact]
    public void CanUnlinkProvider_ShouldReturnTrueWhenHasLocalPassword()
    {
        // Arrange
        var user = new User();
        user.SetPassword("password");
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Google" });

        // Act
        var canUnlink = user.CanUnlinkProvider("Google");

        // Assert
        Assert.True(canUnlink);
    }

    [Fact]
    public void CanUnlinkProvider_ShouldReturnTrueWhenHasMultipleProviders()
    {
        // Arrange
        var user = new User();
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Google" });
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Facebook" });

        // Act
        var canUnlink = user.CanUnlinkProvider("Google");

        // Assert
        Assert.True(canUnlink);
    }

    [Fact]
    public void CanUnlinkProvider_ShouldReturnFalseWhenOnlyOneProviderAndNoLocalPassword()
    {
        // Arrange
        var user = new User();
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Google" });

        // Act
        var canUnlink = user.CanUnlinkProvider("Google");

        // Assert
        Assert.False(canUnlink);
    }
}