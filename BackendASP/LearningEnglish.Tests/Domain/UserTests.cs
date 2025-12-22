using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

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
    public void DisplayName_WithEmptyFirstName_ReturnsLastNameOnly()
    {
        // Arrange
        var user = new User
        {
            FirstName = "",
            LastName = "Trần"
        };

        // Act
        var displayName = user.DisplayName;

        // Assert
        Assert.Equal("Trần", displayName);
    }

    [Fact]
    public void DisplayName_WithEmptyLastName_ReturnsFirstNameOnly()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Minh",
            LastName = ""
        };

        // Act
        var displayName = user.DisplayName;

        // Assert
        Assert.Equal("Minh", displayName);
    }

    [Fact]
    public void DisplayName_WithBothEmpty_ReturnsEmptyString()
    {
        // Arrange
        var user = new User
        {
            FirstName = "",
            LastName = ""
        };

        // Act
        var displayName = user.DisplayName;

        // Assert
        Assert.Equal("", displayName);
    }

    [Fact]
    public void DisplayName_WithWhitespace_ReturnsTrimmedName()
    {
        // Arrange
        var user = new User
        {
            FirstName = "  Minh  ",
            LastName = "  Trần  "
        };

        // Act
        var displayName = user.DisplayName;

        // Assert
        Assert.Equal("Minh     Trần", displayName); // Trim chỉ xóa khoảng trắng đầu cuối
    }

    [Fact]
    public void FullName_IsSameAsDisplayName()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Minh",
            LastName = "Trần"
        };

        // Act & Assert - FullName và DisplayName phải giống nhau
        Assert.Equal(user.DisplayName, user.FullName);
    }

    [Fact]
    public void SetPassword_WithValidPassword_HashesPassword()
    {
        // Arrange
        var user = new User();
        var password = "MyPassword123";

        // Act
        user.SetPassword(password);

        // Assert - PasswordHash phải khác plaintext password
        Assert.NotNull(user.PasswordHash);
        Assert.NotEqual(password, user.PasswordHash);
        Assert.True(user.PasswordHash!.Length > 0);
    }

    [Fact]
    public void SetPassword_MultipleTimesWithSamePassword_CreatesDifferentHashes()
    {
        // Arrange
        var user1 = new User();
        var user2 = new User();
        var password = "SamePassword123";

        // Act
        user1.SetPassword(password);
        user2.SetPassword(password);

        // Assert - BCrypt tạo salt khác nhau mỗi lần hash
        Assert.NotEqual(user1.PasswordHash, user2.PasswordHash);
    }

    [Fact]
    public void SetPassword_OverwritesPreviousPassword()
    {
        // Arrange
        var user = new User();
        user.SetPassword("OldPassword");
        var oldHash = user.PasswordHash;

        // Act - Đổi password mới
        user.SetPassword("NewPassword");

        // Assert - Hash mới khác hash cũ
        Assert.NotEqual(oldHash, user.PasswordHash);
        Assert.False(user.VerifyPassword("OldPassword"));
        Assert.True(user.VerifyPassword("NewPassword"));
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
    public void VerifyPassword_WithNullPasswordHash_ReturnsFalse()
    {
        // Arrange - User không có password
        var user = new User();

        // Act
        var result = user.VerifyPassword("AnyPassword");

        // Assert - Phải trả về false khi chưa set password
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_IsCaseSensitive()
    {
        // Arrange
        var user = new User();
        user.SetPassword("Password123");

        // Act & Assert - Phải phân biệt hoa thường
        Assert.True(user.VerifyPassword("Password123"));
        Assert.False(user.VerifyPassword("password123"));
        Assert.False(user.VerifyPassword("PASSWORD123"));
    }

    [Fact]
    public void VerifyPassword_WithEmptyString_ReturnsFalse()
    {
        // Arrange
        var user = new User();
        user.SetPassword("ValidPassword");

        // Act
        var result = user.VerifyPassword("");

        // Assert
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

    [Fact]
    public void CanUnlinkProvider_WhenHasLocalPassword_ReturnsTrue()
    {
        // Arrange - User có password local và 1 external login
        var user = new User();
        user.SetPassword("LocalPassword123");
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Google" });

        // Act - Có thể unlink vì còn local password
        var result = user.CanUnlinkProvider("Google");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanUnlinkProvider_IsCaseInsensitive()
    {
        // Arrange
        var user = new User();
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Google" });
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Facebook" });

        // Act - Test với các dạng chữ hoa/thường khác nhau
        var result1 = user.CanUnlinkProvider("GOOGLE");
        var result2 = user.CanUnlinkProvider("google");
        var result3 = user.CanUnlinkProvider("GoOgLe");

        // Assert - Tất cả phải trả về true
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
    }

    // ===== Property Tests =====
    [Fact]
    public void User_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var user = new User();

        // Assert - Kiểm tra các giá trị mặc định
        Assert.Equal(string.Empty, user.FirstName);
        Assert.Equal(string.Empty, user.Email);
        Assert.True(user.IsMale);
        Assert.False(user.EmailVerified);
        Assert.Equal(AccountStatus.Active, user.Status);
        Assert.NotNull(user.Roles);
        Assert.Empty(user.Roles);
        Assert.NotNull(user.ExternalLogins);
        Assert.Empty(user.ExternalLogins);
    }

    [Fact]
    public void User_CreatedAt_IsSetToUtcNow()
    {
        // Arrange & Act
        var beforeCreation = DateTime.UtcNow;
        var user = new User();
        var afterCreation = DateTime.UtcNow;

        // Assert - CreatedAt phải nằm trong khoảng thời gian tạo
        Assert.True(user.CreatedAt >= beforeCreation);
        Assert.True(user.CreatedAt <= afterCreation);
    }

    [Fact]
    public void User_UpdatedAt_IsSetToUtcNow()
    {
        // Arrange & Act
        var beforeCreation = DateTime.UtcNow;
        var user = new User();
        var afterCreation = DateTime.UtcNow;

        // Assert - UpdatedAt phải nằm trong khoảng thời gian tạo
        Assert.True(user.UpdatedAt >= beforeCreation);
        Assert.True(user.UpdatedAt <= afterCreation);
    }

    [Fact]
    public void User_PropertiesCanBeSet()
    {
        // Arrange & Act
        var user = new User
        {
            UserId = 1,
            FirstName = "Minh",
            LastName = "Trần",
            Email = "minh@example.com",
            NormalizedEmail = "MINH@EXAMPLE.COM",
            PhoneNumber = "0901234567",
            DateOfBirth = new DateTime(2000, 1, 1),
            IsMale = true,
            EmailVerified = true,
            AvatarKey = "avatar123",
            Status = AccountStatus.Suspended
        };

        // Assert - Tất cả properties phải được set đúng
        Assert.Equal(1, user.UserId);
        Assert.Equal("Minh", user.FirstName);
        Assert.Equal("Trần", user.LastName);
        Assert.Equal("minh@example.com", user.Email);
        Assert.Equal("MINH@EXAMPLE.COM", user.NormalizedEmail);
        Assert.Equal("0901234567", user.PhoneNumber);
        Assert.Equal(new DateTime(2000, 1, 1), user.DateOfBirth);
        Assert.True(user.IsMale);
        Assert.True(user.EmailVerified);
        Assert.Equal("avatar123", user.AvatarKey);
        Assert.Equal(AccountStatus.Suspended, user.Status);
    }

    // ===== Complex Scenario Tests =====
    [Fact]
    public void User_WithBothLocalAndExternalAuth_CanAuthenticateBothWays()
    {
        // Arrange - User có cả local password và external login
        var user = new User();
        user.SetPassword("LocalPassword123");
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Google", ProviderUserId = "google123" });

        // Act & Assert - Có thể authenticate bằng cả 2 cách
        Assert.True(user.HasLocalPassword());
        Assert.True(user.HasExternalLogin("Google"));
        Assert.True(user.VerifyPassword("LocalPassword123"));
        Assert.False(user.IsExternalUserOnly());
    }

    [Fact]
    public void User_ConvertFromExternalOnlyToHybrid_WorksCorrectly()
    {
        // Arrange - User chỉ có external login
        var user = new User();
        user.ExternalLogins.Add(new ExternalLogin { Provider = "Google" });
        Assert.True(user.IsExternalUserOnly());

        // Act - Thêm local password
        user.SetPassword("NewPassword123");

        // Assert - Bây giờ là hybrid user
        Assert.False(user.IsExternalUserOnly());
        Assert.True(user.HasLocalPassword());
        Assert.True(user.HasExternalLogin("Google"));
        Assert.True(user.CanUnlinkProvider("Google"));
    }

    [Fact]
    public void User_WithMultipleExternalLogins_ManagesThemCorrectly()
    {
        // Arrange
        var user = new User();
        var googleLogin = new ExternalLogin { Provider = "Google", ProviderUserId = "google123" };
        var facebookLogin = new ExternalLogin { Provider = "Facebook", ProviderUserId = "fb456" };
        
        user.ExternalLogins.Add(googleLogin);
        user.ExternalLogins.Add(facebookLogin);

        // Act & Assert
        Assert.Equal(2, user.ExternalLogins.Count);
        Assert.True(user.HasExternalLogin("Google"));
        Assert.True(user.HasExternalLogin("Facebook"));
        Assert.False(user.HasExternalLogin("Twitter"));
        
        Assert.Equal(googleLogin, user.GetExternalLogin("Google"));
        Assert.Equal(facebookLogin, user.GetExternalLogin("Facebook"));
        
        Assert.True(user.CanUnlinkProvider("Google"));
        Assert.True(user.CanUnlinkProvider("Facebook"));
    }
}
