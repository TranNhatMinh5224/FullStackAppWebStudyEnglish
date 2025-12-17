using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    // Interface cho Google OAuth2 provider (Infrastructure layer)
    public interface IGoogleAuthProvider
    {
        // Lấy thông tin user từ authorization code
        Task<GoogleUserInfo?> GetUserInfoFromCodeAsync(string code);
    }

    // Model chứa thông tin user từ Google
    public class GoogleUserInfo
    {
        public string Subject { get; set; } = string.Empty;  // Google User ID (sub)
        public string Email { get; set; } = string.Empty;
        public string? GivenName { get; set; }
        public string? FamilyName { get; set; }
        public string? Name { get; set; }
        public string? Picture { get; set; }
        public bool EmailVerified { get; set; }
    }
}
