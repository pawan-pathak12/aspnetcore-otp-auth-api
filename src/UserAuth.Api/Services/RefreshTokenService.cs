using UserAuth.Api.Data;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Repository;
using UserAuth.Api.Interfaces.Service;

namespace UserAuth.Api.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        //        private readonly AppDbContext _context;

        public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository, AppDbContext context)
        {
            _refreshTokenRepository = refreshTokenRepository;
            //          this._context = context;
        }

        public async Task<bool> CreateTokenAsync(RefreshToken token)
        {
            var id = await _refreshTokenRepository.AddAsync(token);
            if (id <= 0)
            {
                return false;
            }
            return true;
        }

        public async Task<RefreshToken?> ValidateTokenAsync(string token)
        {
            return await _refreshTokenRepository.GetByTokenAsync(token);
        }

        public async Task<IEnumerable<RefreshToken>> GetUserTokensAsync(int userId)
        {
            return await _refreshTokenRepository.GetAllByUserIdAsync(userId);
        }

        public async Task<bool> RevokeTokenAsync(int id)
        {
            return await _refreshTokenRepository.RevokeAsync(id);
        }

        public async Task<bool> DeleteTokenAsync(int id)
        {
            return await _refreshTokenRepository.DeleteAsync(id);
        }
        // later convert token to tokenhash
        public async Task<RefreshToken?> GetDataByTokenAsync(string token)
        {
            return await _refreshTokenRepository.GetByTokenAsync(token);

        }

        public async Task<bool> UpdateAsync(int id, RefreshToken refreshToken)
        {
            return await _refreshTokenRepository.UpdateAsync(id, refreshToken);
        }
    }
}
