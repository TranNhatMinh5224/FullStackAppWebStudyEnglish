using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Infrastructure.Data;

/// <summary>
/// Seed data cho Admin Permissions
/// SuperAdmin (Role) → Full quyền tự động, không thể thu hồi
/// Content Admin → Admin role + Content permissions
/// Finance Admin → Admin role + Finance permissions
/// Full Admin → Admin role + ALL permissions (có thể thu hồi riêng lẻ)
/// </summary>
public static class AdminPermissionSeeder
{
    public static List<Permission> GetAdminPermissions()
    {
        return new List<Permission>
        {
            // ==========================================
            // CONTENT MANAGEMENT (Content Admin)
            // ==========================================
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

            // ==========================================
            // USER & FINANCE MANAGEMENT (Finance Admin)
            // ==========================================
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

            // ==========================================
            // SYSTEM (CHỈ SUPER ADMIN)
            // ==========================================
            new() 
            { 
                PermissionId = 8,
                Name = "Admin.System.FullAccess", 
                DisplayName = "Toàn quyền hệ thống", 
                Category = "System",
                Description = "Super Admin - full permissions, không thể thu hồi"
            }
        };
    }

    /// <summary>
    /// RolePermissions mapping - Gán permissions mặc định cho roles
    /// SuperAdmin: Tự động có full quyền qua Role, không cần RolePermissions
    /// Admin role: Không có permission mặc định, phải gán thủ công
    /// </summary>
    public static List<RolePermission> GetDefaultRolePermissions()
    {
        var fixedAssignedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        
        return new List<RolePermission>
        {
            // SuperAdmin (RoleId = 1) có tất cả permissions
            new() { RoleId = 1, PermissionId = 1, AssignedAt = fixedAssignedAt },
            new() { RoleId = 1, PermissionId = 2, AssignedAt = fixedAssignedAt },
            new() { RoleId = 1, PermissionId = 3, AssignedAt = fixedAssignedAt },
            new() { RoleId = 1, PermissionId = 4, AssignedAt = fixedAssignedAt },
            new() { RoleId = 1, PermissionId = 5, AssignedAt = fixedAssignedAt },
            new() { RoleId = 1, PermissionId = 6, AssignedAt = fixedAssignedAt },
            new() { RoleId = 1, PermissionId = 7, AssignedAt = fixedAssignedAt },
            new() { RoleId = 1, PermissionId = 8, AssignedAt = fixedAssignedAt }
            
            // Admin role (RoleId = 2): KHÔNG có permissions mặc định
            // Permissions sẽ được gán thủ công tùy theo loại admin:
            // - Content Admin: permissions 1, 2, 3
            // - Finance Admin: permissions 4, 5, 6, 7
            // - Full Admin: permissions 1-7 (không có permission 8)
        };
    }

    /// <summary>
    /// Helper method để lấy permission IDs theo category
    /// </summary>
    public static Dictionary<string, List<int>> GetPermissionsByCategory()
    {
        return new Dictionary<string, List<int>>
        {
            ["Content"] = new List<int> { 1, 2, 3 },      // Content Admin permissions
            ["Finance"] = new List<int> { 4, 5, 6, 7 },   // Finance Admin permissions
            ["System"] = new List<int> { 8 },             // SuperAdmin only
            ["All"] = new List<int> { 1, 2, 3, 4, 5, 6, 7 } // Full Admin (không có System)
        };
    }
}
