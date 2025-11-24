using Microsoft.Extensions.Configuration;

namespace LearningEnglish.Application.Common.Helpers
{
    public static class BuildPublicUrl
    {
        private static string? _baseUrl;

        public static void Configure(IConfiguration config)
        {
            _baseUrl = config["Minio:BaseUrl"];
        }

        public static string BuildURL(string bucketName, string objectKey)
        {
            if (string.IsNullOrEmpty(_baseUrl))
                throw new Exception("BuildPublicUrl not configured. Call BuildPublicUrl.Configure(config).");

            return $"{_baseUrl}/{bucketName}/{objectKey}".Replace("\\", "/");
        }
    }
}
