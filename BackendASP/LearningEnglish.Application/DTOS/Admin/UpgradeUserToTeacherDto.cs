namespace LearningEnglish.Application.DTOs.Admin
{
    // DTO để Admin/SuperAdmin nâng cấp user thành Teacher (gán role + tạo subscription)
    // Dùng khi thanh toán thất bại hoặc cần xử lý thủ công
    public class UpgradeUserToTeacherDto
    {
        /// <summary>
        /// Email của user cần nâng cấp
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// ID của TeacherPackage (bắt buộc - phải chỉ định gói nào)
        /// </summary>
        public int TeacherPackageId { get; set; }
    }
}

