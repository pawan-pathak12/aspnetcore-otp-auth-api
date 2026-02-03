using UserAuth.Api.DTOs;

namespace UserAuth.Api.Interfaces.Service
{
    public interface IAuthService
    {
        Task<(bool success, string errorMessage, AuthResponseDto? response)> RefreshToken(string refreshToken);
    }
}
