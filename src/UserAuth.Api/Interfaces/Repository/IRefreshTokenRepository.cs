using UserAuth.Api.Entities;

namespace UserAuth.Api.Interfaces.Repository
{
    public interface IRefreshTokenRepository
    {
        Task<int> AddAsync(RefreshToken token);
        Task<RefreshToken?> GetByIdAsync(int id);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetAllByUserIdAsync(int userId);
        Task<bool> RevokeAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
