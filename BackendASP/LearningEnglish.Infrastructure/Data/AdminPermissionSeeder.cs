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
    public static List<RolePermission> GetDefaultRolePermissions()
    {
        var fixedAssignedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        return new List<RolePermission>
        {
            new() { RoleId = 1, PermissionId = 1, AssignedAt = fixedAssignedAt },
            new() { RoleId = 1, PermissionId = 2, AssignedAt = fixedAssignedAt },
            new() { RoleId = 1, PermissionId = 3, AssignedAt = fixedAssignedAt },
            new() { RoleId = 1, PermissionId = 4, AssignedAt = fixedAssignedAt },
            new() { RoleId = 1, PermissionId = 5, AssignedAt = fixedAssignedAt },
            new() { RoleId = 1, PermissionId = 6, AssignedAt = fixedAssignedAt },
            new() { RoleId = 1, PermissionId = 7, AssignedAt = fixedAssignedAt },
            new() { RoleId = 1, PermissionId = 8, AssignedAt = fixedAssignedAt }
        };
    }

    // Helper map permissions theo nhóm
    public static Dictionary<string, List<int>> GetPermissionsByCategory()
    {
        return new()
        {
            ["Content"] = new() { 1, 2, 3 },
            ["Finance"] = new() { 4, 5, 6, 7 },
            ["System"]  = new() { 8 },
            ["All"]     = new() { 1, 2, 3, 4, 5, 6, 7 }
        };
    }
}
