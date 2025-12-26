using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Infrastructure.Data;

public static class AdminPermissionSeeder
{
    // Seed danh sách permissions cho admin
    public static List<Permission> GetAdminPermissions()
    {
        return new List<Permission>
        {
            // CONTENT
            new()
            {
                PermissionId = 1,
                Name = "Admin.Course.Manage",
                DisplayName = "Quản lý khóa học",
                Category = "Content",
                Description = "Tạo, sửa, xóa, publish khóa học"
            },
            new()
            {
                PermissionId = 9,
                Name = "Admin.Course.Enroll",
                DisplayName = "Quản lý học viên trong khóa học",
                Category = "Finance",
                Description = "Thêm/xóa học viên vào khóa học (dùng khi thanh toán lỗi, nâng cấp user)"
            },
            new()
            {
                PermissionId = 2,
                Name = "Admin.Lesson.Manage",
                DisplayName = "Quản lý bài học",
                Category = "Content",
                Description = "Tạo, sửa, xóa lessons và modules"
            },
            new()
            {
                PermissionId = 3,
                Name = "Admin.Content.Manage",
                DisplayName = "Quản lý nội dung",
                Category = "Content",
                Description = "Quản lý flashcards, quizzes, essays, assets frontend"
            },

            // FINANCE
            new()
            {
                PermissionId = 4,
                Name = "Admin.User.Manage",
                DisplayName = "Quản lý người dùng",
                Category = "Finance",
                Description = "Xem, block/unblock, xóa users, gán roles"
            },
            new()
            {
                PermissionId = 5,
                Name = "Admin.Payment.Manage",
                DisplayName = "Quản lý thanh toán",
                Category = "Finance",
                Description = "Xem payments, hoàn tiền, fix lỗi thanh toán"
            },
            new()
            {
                PermissionId = 6,
                Name = "Admin.Revenue.View",
                DisplayName = "Xem doanh thu",
                Category = "Finance",
                Description = "Xem báo cáo doanh thu và thống kê tài chính"
            },
            new()
            {
                PermissionId = 7,
                Name = "Admin.Package.Manage",
                DisplayName = "Quản lý gói giáo viên",
                Category = "Finance",
                Description = "Tạo, sửa, xóa teacher packages"
            },

            // SYSTEM
            new()
            {
                PermissionId = 8,
                Name = "Admin.System.FullAccess",
                DisplayName = "Toàn quyền hệ thống",
                Category = "System",
                Description = "Super Admin - full permissions"
            }
        };
    }

    // Gán permission mặc định cho role
    // RoleId: 1=SuperAdmin, 2=ContentAdmin, 3=FinanceAdmin
    public static List<RolePermission> GetDefaultRolePermissions()
    {
        var fixedAssignedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        return new List<RolePermission>
        {
            // SuperAdmin (RoleId = 1): Tất cả permissions
            new() { RoleId = 1, PermissionId = 1, AssignedAt = fixedAssignedAt }, // Course Manage
            new() { RoleId = 1, PermissionId = 2, AssignedAt = fixedAssignedAt }, // Lesson
            new() { RoleId = 1, PermissionId = 3, AssignedAt = fixedAssignedAt }, // Content
            new() { RoleId = 1, PermissionId = 4, AssignedAt = fixedAssignedAt }, // User
            new() { RoleId = 1, PermissionId = 5, AssignedAt = fixedAssignedAt }, // Payment
            new() { RoleId = 1, PermissionId = 6, AssignedAt = fixedAssignedAt }, // Revenue
            new() { RoleId = 1, PermissionId = 7, AssignedAt = fixedAssignedAt }, // Package
            new() { RoleId = 1, PermissionId = 8, AssignedAt = fixedAssignedAt }, // System
            new() { RoleId = 1, PermissionId = 9, AssignedAt = fixedAssignedAt }, // Course Enroll
            
            // ContentAdmin (RoleId = 2): Content permissions (1,2,3)
            // - Quản lý Course (tạo, sửa, xóa), Lesson, Content
            new() { RoleId = 2, PermissionId = 1, AssignedAt = fixedAssignedAt }, // Course Manage
            new() { RoleId = 2, PermissionId = 2, AssignedAt = fixedAssignedAt }, // Lesson
            new() { RoleId = 2, PermissionId = 3, AssignedAt = fixedAssignedAt }, // Content
            
            // FinanceAdmin (RoleId = 3): Finance permissions (4,5,6,7) + Course Enroll (9)
            // - Quản lý User, Payment, Revenue, Package
            // - Thêm/xóa học viên vào course (khi thanh toán lỗi, nâng cấp user lên teacher)
            // - KHÔNG có quyền tạo/sửa/xóa course
            new() { RoleId = 3, PermissionId = 9, AssignedAt = fixedAssignedAt }, // Course Enroll (thêm/xóa user)
            new() { RoleId = 3, PermissionId = 4, AssignedAt = fixedAssignedAt }, // User
            new() { RoleId = 3, PermissionId = 5, AssignedAt = fixedAssignedAt }, // Payment
            new() { RoleId = 3, PermissionId = 6, AssignedAt = fixedAssignedAt }, // Revenue
            new() { RoleId = 3, PermissionId = 7, AssignedAt = fixedAssignedAt }  // Package
        };
    }

    // Helper map permissions theo nhóm
    public static Dictionary<string, List<int>> GetPermissionsByCategory()
    {
        return new()
        {
            ["Content"] = new() { 1, 2, 3 },           // Course Manage, Lesson, Content
            ["Finance"] = new() { 4, 5, 6, 7, 9 },     // User, Payment, Revenue, Package, Course Enroll
            ["System"]  = new() { 8 },                 // System FullAccess
            ["All"]     = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9 } // Tất cả (SuperAdmin)
        };
    }
}
