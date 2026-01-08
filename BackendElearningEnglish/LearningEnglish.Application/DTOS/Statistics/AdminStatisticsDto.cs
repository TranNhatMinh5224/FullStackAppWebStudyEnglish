namespace LearningEnglish.Application.DTOs
{
    // DTO cho tổng quan thống kê Admin Dashboard (chỉ User + Revenue)
    public class AdminOverviewStatisticsDto
    {
        // Tổng số users theo từng role
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalAdmins { get; set; }
        
        // Tổng doanh thu
        public decimal TotalRevenue { get; set; }
        
        // Users mới trong 30 ngày
        public int NewUsersLast30Days { get; set; }
    }
    
    // DTO cho user statistics chi tiết
    public class UserStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalAdmins { get; set; }
        public int ActiveUsers { get; set; } // Users không bị block
        public int BlockedUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
    }

    // DTO cho Revenue statistics chi tiết
    public class RevenueStatisticsDto
    {
        // Tổng doanh thu
        public decimal TotalRevenue { get; set; }
        
        // Doanh thu đã hoàn thành (Completed payments)
        public decimal CompletedRevenue { get; set; }
        
        // Doanh thu đang chờ (Pending payments)
        public decimal PendingRevenue { get; set; }
        
        // Doanh thu theo thời gian
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisWeek { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueThisYear { get; set; }
        
        // Số lượng giao dịch
        public int TotalTransactions { get; set; }
        public int CompletedTransactions { get; set; }
        public int PendingTransactions { get; set; }
        public int FailedTransactions { get; set; }
        
        // Trung bình
        public decimal AverageTransactionValue { get; set; }
        
        // Top statistics
        public int TransactionsToday { get; set; }
        public int TransactionsThisWeek { get; set; }
        public int TransactionsThisMonth { get; set; }
    }

    // DTO cho Revenue Chart Data (simplified for charts)
    public class RevenueChartDto
    {
        // Tổng doanh thu
        public decimal TotalRevenue { get; set; }
        
        // Doanh thu theo loại sản phẩm (for pie chart / breakdown)
        public decimal CourseRevenue { get; set; }
        public decimal TeacherPackageRevenue { get; set; }
        
        // Timeline data (for line/bar chart)
        public List<RevenueTimelineItem> DailyRevenue { get; set; } = new();
        public List<RevenueTimelineItem> MonthlyRevenue { get; set; } = new();
        
        // Timeline breakdown by ProductType
        public List<RevenueTimelineItem> DailyCourseRevenue { get; set; } = new();
        public List<RevenueTimelineItem> DailyTeacherPackageRevenue { get; set; } = new();
    }

    public class RevenueTimelineItem
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }
}
