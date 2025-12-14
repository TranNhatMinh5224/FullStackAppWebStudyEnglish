namespace LearningEnglish.Application.Configurations
{
    public class AzureSpeechOptions
    {
        public string SubscriptionKey { get; set; } = string.Empty;  // Đổi tên cho khớp với appsettings
        public string Region { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
