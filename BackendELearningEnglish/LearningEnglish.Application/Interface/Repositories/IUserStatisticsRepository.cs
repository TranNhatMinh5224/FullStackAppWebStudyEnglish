namespace LearningEnglish.Application.Interface
{
    /// <summary>
    /// Repository cho User Statistics (tách riêng để dễ quản lý và maintain)
    /// </summary>
    public interface IUserStatisticsRepository
    {
        // User count statistics
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetUserCountByRoleAsync(string roleName);
        Task<int> GetActiveUsersCountAsync(); // Users không bị block
        Task<int> GetBlockedUsersCountAsync();
        Task<int> GetNewUsersCountAsync(DateTime fromDate);
    }
}

