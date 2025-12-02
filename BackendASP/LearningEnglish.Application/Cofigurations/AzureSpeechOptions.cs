namespace LearningEnglish.Application.Configurations
{
    public class AzureSpeechOptions
    {
        public string Key { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
