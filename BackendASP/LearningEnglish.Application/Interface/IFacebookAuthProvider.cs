namespace LearningEnglish.Application.Interface
{
    // Interface cho Facebook OAuth2 provider (Infrastructure layer)
    public interface IFacebookAuthProvider
    {
        // Lấy thông tin user từ authorization code
        Task<FacebookUserInfo?> GetUserInfoFromCodeAsync(string code);
    }

    // Model chứa thông tin user từ Facebook
    public class FacebookUserInfo
    {
        public string Id { get; set; } = string.Empty;  // Facebook User ID
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PictureUrl { get; set; }
    }
}
