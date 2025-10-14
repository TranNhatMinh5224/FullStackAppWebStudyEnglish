using CleanDemo.Domain.Entities;

namespace CleanDemo.Application.Service.Auth.Token
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        RefreshToken GenerateRefreshToken(User user);
    }
}
