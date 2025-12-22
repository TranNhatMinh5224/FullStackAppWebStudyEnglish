using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IGoogleAuthProvider
    {
        // Lấy thông tin Google user
        Task<GoogleUserInfo?> GetUserInfoFromCodeAsync(string code);
    }

    public class GoogleUserInfo
    {
        public string Subject { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? GivenName { get; set; }
        public string? FamilyName { get; set; }
        public string? Name { get; set; }
        public string? Picture { get; set; }
        public bool EmailVerified { get; set; }
    }
}
