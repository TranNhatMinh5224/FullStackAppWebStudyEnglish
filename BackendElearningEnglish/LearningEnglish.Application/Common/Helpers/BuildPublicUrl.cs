using Microsoft.Extensions.Configuration;

namespace LearningEnglish.Application.Common.Helpers
{
    /// <summary>
    /// Application-level helper for building public URLs for MinIO storage
    /// Infrastructure services use: LearningEnglish.Infrastructure.Common.Helpers.BuildPublicUrl
    /// Application services should prefer using Image Services (ICourseImageService.BuildImageUrl, etc.)
    /// </summary>
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
                throw new InvalidOperationException("BuildPublicUrl not configured. Call BuildPublicUrl.Configure(config) in Program.cs.");

            return $"{_baseUrl}/{bucketName}/{objectKey}".Replace("\\", "/");
        }
    }
}
