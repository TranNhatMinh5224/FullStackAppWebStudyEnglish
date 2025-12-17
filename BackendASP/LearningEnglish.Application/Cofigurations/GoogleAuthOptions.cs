namespace LearningEnglish.Application.Cofigurations
{
    public class GoogleAuthOptions
    {
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? RedirectUri { get; set; }  // OAuth2 redirect URI
    }
}