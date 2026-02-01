using UserAuth.Api.Entities;

namespace UserAuth.Api.Interfaces.Service
{
    public interface IRefreshTokenService
    {
        Task<bool> CreateTokenAsync(RefreshToken token);
        Task<RefreshToken?> ValidateTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetUserTokensAsync(int userId);
        Task<bool> RevokeTokenAsync(int id);
        Task<bool> DeleteTokenAsync(int id);
        Task<RefreshToken?> GetDataByTokenAsync(string token);


    }
}
