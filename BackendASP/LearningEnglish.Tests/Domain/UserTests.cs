using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Tests.Domain;

public class UserTests
{
    [Fact]
    public void DisplayName_CombinesFirstAndLastName_ReturnsFullName()
    {
        // Arrange - Chuẩn bị dữ liệu
        var user = new User
        {
            FirstName = "Minh",
            LastName = "Trần"
        };

        // Act - Thực hiện hành động
        var displayName = user.DisplayName;

        // Assert - Kiểm tra kết quả
        Assert.Equal("Minh Trần", displayName);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange - Tạo user và set password
        var user = new User();
        user.SetPassword("MyPassword123");

        // Act - Verify với mật khẩu đúng
        var result = user.VerifyPassword("MyPassword123");

        // Assert - Phải trả về true
        Assert.True(result);
    }
    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange - Tạo user và set password
        var user = new User();
        user.SetPassword("matkhausai123");

        // Act - Verify với mật khẩu sai
        var result = user.VerifyPassword("SaiMatKhau");

        // Assert - Phải trả về false
        Assert.False(result);
    }

    [Fact]
    public void HashLocalPassword_PasswordIsHashed_VerifyReturnsTrue()
    {
        // Arrange
        var user = new User();
        user.SetPassword("123456");

        // Act
        var result = user.HasLocalPassword();

        // Assert
        Assert.True(result);

    }
    [Fact]
    public void HashLocalPassword_PasswordIsNotHashed_VerifyReturnsFalse()
    {
        // Arrange
        var user = new User();

        // Act
        var result = user.HasLocalPassword();

        // Assert
        Assert.False(result);

    }
    // Test có đăng nhập bên ngoài với nhà cung cấp tồn tại
    [Fact]
    public void HasExternalLogin_WhenProviderExists_ReturnsTrue()
    {
        // Arrange
        var user = new User();
        user.ExternalLogins.Add(new ExternalLogin
        {
            Provider = "Google"
        });

        // Act
        var result = user.HasExternalLogin("google");

        // Assert
        Assert.True(result);
    }
    // Có đăng nhập bên ngoài với nhà cung cấp không tồn tại
    [Fact]
    public void HasExternalLogin_WhenProviderDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var user = new User();

        // Act
        var result = user.HasExternalLogin("Zalo");

        // Assert
        Assert.False(result);
    }
    // Test có đăng nhập bên ngoài với chữ hoa chữ thường
    [Fact]
    public void HasExternalLogin_IsCaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var user = new User();
        user.ExternalLogins.Add(new ExternalLogin
        {
            Provider = "GOOGLE"
        });

        // Act
        var result = user.HasExternalLogin("google");

        // Assert
        Assert.True(result);
    }
    // đăng nhập bên ngoài khi nhà cung cấp tồn tại trả về là đã login từ bên ngoài
    [Fact]
    public void GetExternalLogin_WhenProviderExists_ReturnsExternalLogin()
    {
        // Arrange
        var user = new User();
        var login = new ExternalLogin { Provider = "Google" };
        user.ExternalLogins.Add(login);

        // Act
        var result = user.GetExternalLogin("google");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Google", result!.Provider);
    }
    // đăng nhập bên ngoài khi nhà cung cấp không tồn tại trả về null
    [Fact]
    public void GetExternalLogin_WhenProviderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var user = new User();

        // Act
        var result = user.GetExternalLogin("Facebook");

        // Assert
        Assert.Null(result);
    }
    // Test ràng buộc
    // user chỉ đăng nhập bên ngoài thì trả về true
    [Fact]
    public void IsExternalUserOnly_WhenOnlyExternalLogin_ReturnsTrue()
    {
        // Arrange
        var user = new User();
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Google" });

        // Act
        var result = user.IsExternalUserOnly();

        // Assert
        Assert.True(result);
    }
    // user chỉ đăng nhập bên ngoài khi có password thì trả về false
    [Fact]
    public void IsExternalUserOnly_WhenHasLocalPassword_ReturnsFalse()
    {
        // Arrange
        var user = new User();
        user.SetPassword("123456");
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Google" });

        // Act
        var result = user.IsExternalUserOnly();

        // Assert
        Assert.False(result);
    }
    // hủy liên kết nhà cung cấp
    [Fact]
    public void CanUnlinkProvider_WhenOnlyOneExternalLogin_ReturnsFalse()
    {
        // Arrange
        var user = new User();
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Google" });

        // Act
        var result = user.CanUnlinkProvider("Google");

        // Assert
        Assert.False(result);
    }
    // hủy liên kết nhà cung cấp khi có nhiều đăng nhập bên ngoài
    [Fact]
    public void CanUnlinkProvider_WhenMultipleExternalLogins_ReturnsTrue()
    {
        // Arrange
        var user = new User();
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Google" });
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Facebook" });

        // Act
        var result = user.CanUnlinkProvider("Google");

        // Assert
        Assert.True(result);
    }
}
