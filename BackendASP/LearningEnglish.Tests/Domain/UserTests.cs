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
        Assert.Equal("Minh Nguyen", displayName);
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
}
