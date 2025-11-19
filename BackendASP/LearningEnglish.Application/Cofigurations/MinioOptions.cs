namespace LearningEnglish.Application.Configurations
{
    public class MinioOptions
    {
        public string Endpoint { get; set; } = string.Empty;  // endpoint URL  của MinIO server
        public string AccessKey { get; set; } = string.Empty;  // Access key để xác thực
        public string SecretKey { get; set; } = string.Empty;  // Secret key để xác thực
        public bool UseSSL { get; set; } = false;  // Sử dụng SSL hay không
        public string BaseUrl { get; set; } = string.Empty;  // URL cơ sở cho MinIO
    }
}
