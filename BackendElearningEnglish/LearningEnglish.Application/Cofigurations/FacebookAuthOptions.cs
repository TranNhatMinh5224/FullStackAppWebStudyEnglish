namespace LearningEnglish.Application.Cofigurations
{
    public class FacebookAuthOptions
    {
        public string AppId { get; set; } = string.Empty;
        public string AppSecret { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;  // OAuth2 redirect URI
    }
}
