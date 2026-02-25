using UserAuth.Api.Entities;
using UserAuth.Api.Results;

namespace UserAuth.Api.Interfaces.Service
{
    public interface IAuthService
    {
        #region JWT

        Task<AuthResult> RotateRefreshTokenAsync(string refreshToken);
        Task<AuthResult> LoginAsync(string email, string password);
        Task<string> LogoutSessionAsync(string refreshToken);
        Task<AuthResult> RegisterAsync(User user);
        #endregion

        #region OTP 
        Task<AuthResult> SendOtpAsync(string email);
        Task<AuthResult> VerifyOtpAsync(string otp, string email);
        #endregion

    }
}
