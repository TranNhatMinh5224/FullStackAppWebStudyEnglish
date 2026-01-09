namespace LearningEnglish.Application.Interface
{
    public interface IFacebookAuthProvider
    {
        // Lấy thông tin Facebook user
        Task<FacebookUserInfo?> GetUserInfoFromCodeAsync(string code);
    }

    public class FacebookUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PictureUrl { get; set; }
    }
}
