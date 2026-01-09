namespace LearningEnglish.Application.Interface.Infrastructure;

/// <summary>
/// Interface để cung cấp cấu hình storage buckets
/// Tuân thủ Clean Architecture: Application chỉ biết interface, 
/// Implementation trong Infrastructure có bucket names cụ thể
/// </summary>
public interface IStorageConfigProvider
{
    /// <summary>
    /// Lấy danh sách tất cả bucket names cần cleanup temp files
    /// </summary>
    IReadOnlyList<string> GetAllBucketsForCleanup();
}
