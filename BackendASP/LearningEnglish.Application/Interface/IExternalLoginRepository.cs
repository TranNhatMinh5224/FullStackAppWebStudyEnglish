using LearningEnglish.Domain.Entities;


namespace LearningEnglish.Application.Interface
{
    public interface IExternalLoginRepository
    {
        Task<ExternalLogin?> GetByProviderAndUserIdAsync(string provider, string providerUserId); //  xem xem đã từng đăng nhập bằng provider + providerUserId chưa 
        Task<ExternalLogin?> AddAsync(ExternalLogin externalLogin); // thêm mới external login
        Task<bool> DeleteAsync(ExternalLogin externalLogin); // xóa external login
        Task SaveChangesAsync();
    }
}