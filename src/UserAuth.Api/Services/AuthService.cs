using UserAuth.Api.DTOs;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Service;

namespace UserAuth.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthService(ITokenService tokenService, IRefreshTokenService refreshTokenService)
        {
            this._tokenService = tokenService;
            this._refreshTokenService = refreshTokenService;
        }
        // for refresh endpoint 
        public async Task<(bool success, string errorMessage, AuthResponseDto? response)> RefreshToken(string refreshToken)
        {
            var incomingTokenHash = _tokenService.HashToken(refreshToken);

            var existingToken = await _refreshTokenService.GetDataByTokenAsync(incomingTokenHash);

            // check if token exists or not 
            if (existingToken == null)
            {
                return (false, "Token not found ", null);
            }

            // if : reuse refreshToken detection 
            // then revoke all token for that user 
            if (existingToken.IsRevoked)
            {
                await _refreshTokenService.RevokeTokenAsync(existingToken.UserId);
                return (false, "Refresh token reuse detected , please relogin again.", null);
            }
            // check if expired or not
            if (existingToken.ExpiredAt < DateTime.UtcNow)
            {
                return (false, "Refresh token is expired or invalid", null);
            }

            //normal rotation of refreshtoken 
            var accesstoken = _tokenService.GenerateAccessToken(existingToken.User);
            existingToken.IsRevoked = true;

            var plainrefreshToken = _tokenService.GenerateRefreshTokenAsync();
            var hashToken = _tokenService.HashToken(plainrefreshToken);

            var token = new RefreshToken
            {
                UserId = existingToken.UserId,
                TokenHash = hashToken,
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
                ExpiredAt = DateTime.UtcNow.AddDays(7)
            };

            await _refreshTokenService.CreateTokenAsync(token);

            return (true, "", new AuthResponseDto
            {
                AccessToken = accesstoken,
                RefreshToken = hashToken,
                ExpiredAt = token.ExpiredAt
            }
            );
        }
    }
}
