using CleanDemo.Domain.Entities;

namespace CleanDemo.Application.Interface
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        RefreshToken GenerateRefreshToken(User user);
    }
}
