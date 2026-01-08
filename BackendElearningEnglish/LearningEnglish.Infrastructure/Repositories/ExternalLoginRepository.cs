using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace LearningEnglish.Infrastructure.Repositories
{
    public class ExternalLoginRepository : IExternalLoginRepository
    {
        private readonly AppDbContext _context;

        public ExternalLoginRepository(AppDbContext context)
        {
            _context = context;
        }


        // Lấy ExternalLogin theo Provider và ProviderUserId

        public async Task<ExternalLogin?> GetByProviderAndUserIdAsync(string provider, string providerUserId)
        {
            return await _context.ExternalLogins
                .FirstOrDefaultAsync(el => el.Provider == provider && el.ProviderUserId == providerUserId);
        }

        public async Task<ExternalLogin?> AddAsync(ExternalLogin externalLogin)
        {
            _context.ExternalLogins.Add(externalLogin);
            await _context.SaveChangesAsync();
            return externalLogin;
        }

        public async Task<bool> DeleteAsync(ExternalLogin externalLogin)
        {
            _context.ExternalLogins.Remove(externalLogin);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}