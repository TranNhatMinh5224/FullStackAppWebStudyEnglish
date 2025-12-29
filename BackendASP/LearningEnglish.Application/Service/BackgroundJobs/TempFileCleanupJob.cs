using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace LearningEnglish.Application.Service.BackgroundJobs
{
    public class TempFileCleanupJob
    {
        private readonly IMinioClient _minioClient;
        private readonly ILogger<TempFileCleanupJob> _logger;

        // Định nghĩa các bucket cần cleanup
        private readonly string[] _buckets = new[]
        {
            "courses",
            "lessons",
            "lectures",
            "quizgroups",
            "questions",
            "flashcards"
        };

        public TempFileCleanupJob(IMinioClient minioClient, ILogger<TempFileCleanupJob> logger)
        {
            _minioClient = minioClient;
            _logger = logger;
        }

        // Xóa tất cả temp files cũ hơn 24 giờ trong tất cả buckets
        public async Task CleanupOldTempFilesAsync()
        {
            _logger.LogInformation("Starting temp file cleanup job at {Time}", DateTime.UtcNow);

            var cutoffTime = DateTime.UtcNow.AddHours(-24);
            var totalDeleted = 0;
            var totalSize = 0L;

            foreach (var bucketName in _buckets)
            {
                try
                {
                    // Kiểm tra bucket có tồn tại không
                    var bucketExists = await _minioClient.BucketExistsAsync(
                        new BucketExistsArgs().WithBucket(bucketName)
                    );

                    if (!bucketExists)
                    {
                        _logger.LogWarning("Bucket {BucketName} does not exist, skipping", bucketName);
                        continue;
                    }

                    // List tất cả objects trong folder temp/
                    var listArgs = new ListObjectsArgs()
                        .WithBucket(bucketName)
                        .WithPrefix("temp/")
                        .WithRecursive(true);

                    var observable = _minioClient.ListObjectsEnumAsync(listArgs);
                    var objectsToDelete = new List<string>();

                    await foreach (var item in observable)
                    {
                        // Kiểm tra nếu file cũ hơn 24 giờ
                        if (DateTime.TryParse(item.LastModified, out var lastModified) && lastModified < cutoffTime)
                        {
                            objectsToDelete.Add(item.Key);
                            totalSize += (long)item.Size;
                        }
                    }

                    // Xóa các files cũ
                    foreach (var objectKey in objectsToDelete)
                    {
                        try
                        {
                            var removeArgs = new RemoveObjectArgs()
                                .WithBucket(bucketName)
                                .WithObject(objectKey);

                            await _minioClient.RemoveObjectAsync(removeArgs);
                            totalDeleted++;

                            _logger.LogDebug("Deleted temp file: {BucketName}/{ObjectKey}", bucketName, objectKey);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error deleting temp file {BucketName}/{ObjectKey}", bucketName, objectKey);
                        }
                    }

                    if (objectsToDelete.Count > 0)
                    {
                        _logger.LogInformation("Deleted {Count} temp files from bucket {BucketName}",
                            objectsToDelete.Count, bucketName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning up bucket {BucketName}", bucketName);
                }
            }

            var sizeMB = totalSize / 1024.0 / 1024.0;
            _logger.LogInformation(
                "Temp file cleanup completed. Total deleted: {Count} files, {Size:F2} MB",
                totalDeleted, sizeMB
            );
        }

        // Cleanup temp files theo bucket cụ thể
        public async Task CleanupTempFilesByBucketAsync(string bucketName, int hoursOld = 24)
        {
            _logger.LogInformation("Starting temp file cleanup for bucket {BucketName}", bucketName);

            var cutoffTime = DateTime.UtcNow.AddHours(-hoursOld);
            var deletedCount = 0;

            try
            {
                var bucketExists = await _minioClient.BucketExistsAsync(
                    new BucketExistsArgs().WithBucket(bucketName)
                );

                if (!bucketExists)
                {
                    _logger.LogWarning("Bucket {BucketName} does not exist", bucketName);
                    return;
                }

                var listArgs = new ListObjectsArgs()
                    .WithBucket(bucketName)
                    .WithPrefix("temp/")
                    .WithRecursive(true);

                var observable = _minioClient.ListObjectsEnumAsync(listArgs);

                await foreach (var item in observable)
                {
                    if (DateTime.TryParse(item.LastModified, out var lastModified) && lastModified < cutoffTime)
                    {
                        var removeArgs = new RemoveObjectArgs()
                            .WithBucket(bucketName)
                            .WithObject(item.Key);

                        await _minioClient.RemoveObjectAsync(removeArgs);
                        deletedCount++;
                    }
                }

                _logger.LogInformation("Deleted {Count} temp files from bucket {BucketName}", deletedCount, bucketName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up bucket {BucketName}", bucketName);
                throw;
            }
        }
    }
}
