using UserAuth.Api.Data;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Repository;
using UserAuth.Api.Interfaces.Service;

namespace UserAuth.Api.Repository.InMemory
{
    public class InMemoryRefreshTokenRepo : IRefreshTokenRepository
    {
        private readonly InMemoryDbContext _dbContext;
        private readonly ITokenService _tokenService;
        private readonly List<RefreshToken> _tokens;

        public InMemoryRefreshTokenRepo(InMemoryDbContext dbContext, ITokenService tokenService)
        {
            _dbContext = dbContext;
            this._tokenService = tokenService;
            _tokens = _dbContext.RefreshTokens;
        }

        public Task<int> AddAsync(RefreshToken token)
        {
            token.Id = _tokens.Count > 0
                ? _tokens.Max(t => t.Id) + 1
                : 1;

            _tokens.Add(token);
            return Task.FromResult(token.Id);
        }

        public Task<RefreshToken?> GetByIdAsync(int id)
        {
            var token = _tokens.FirstOrDefault(t => t.Id == id);
            return Task.FromResult(token);
        }

        public Task<RefreshToken?> GetByTokenAsync(string token)
        {
            var tokenHash = _tokenService.HashToken(token);
            var found = _tokens.FirstOrDefault(t => t.TokenHash == tokenHash);
            return Task.FromResult(found);
        }

        public Task<IEnumerable<RefreshToken>> GetAllByUserIdAsync(int userId)
        {
            var userTokens = _tokens
                .Where(t => t.UserId == userId)
                .ToList();

            return Task.FromResult<IEnumerable<RefreshToken>>(userTokens.AsReadOnly());
        }

        public Task<bool> RevokeAsync(int id)
        {
            var token = _tokens.FirstOrDefault(t => t.Id == id);
            if (token == null)
            {
                return Task.FromResult(false);
            }

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }

        public Task<bool> UpdateAsync(int id, RefreshToken refreshToken)
        {
            var existing = _tokens.FirstOrDefault(t => t.Id == id);
            if (existing == null)
            {
                return Task.FromResult(false);
            }

            existing.TokenHash = _tokenService.HashToken(refreshToken.TokenHash);
            existing.ExpiredAt = refreshToken.ExpiredAt;
            existing.IsRevoked = refreshToken.IsRevoked;
            existing.RevokedAt = refreshToken.RevokedAt;
            existing.UserId = refreshToken.UserId;

            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(int id)
        {
            var token = _tokens.FirstOrDefault(t => t.Id == id);
            if (token == null)
            {
                return Task.FromResult(false);
            }

            _tokens.Remove(token);
            return Task.FromResult(true);
        }

        public Task<RefreshToken?> GetDataByTokenAsync(string token)
        {
            var tokenData = _tokens
                .Find(x => x.TokenHash == token && !x.IsRevoked);
            if (tokenData == null)
            {
                return null;
            }
            return Task.FromResult(tokenData);
        }
    }
}