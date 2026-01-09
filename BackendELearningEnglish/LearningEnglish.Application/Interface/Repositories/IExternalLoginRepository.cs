using LearningEnglish.Domain.Entities;


namespace LearningEnglish.Application.Interface
{
    public interface IExternalLoginRepository
    {
        // Lấy thông tin đăng nhập ngoài
        Task<ExternalLogin?> GetByProviderAndUserIdAsync(string provider, string providerUserId);
        
        // Thêm đăng nhập ngoài
        Task<ExternalLogin?> AddAsync(ExternalLogin externalLogin);
        
        // Xóa đăng nhập ngoài
        Task<bool> DeleteAsync(ExternalLogin externalLogin);
        
        // Lưu thay đổi
        Task SaveChangesAsync();
    }
}