using UserAuth.Api.Results;

namespace UserAuth.Api.Interfaces.Service
{
    public interface IAuthService
    {

        #region JWT

        Task<AuthResult> RotateRefreshTokenAsync(string refreshToken);
        Task<AuthResult> LoginWithJwtAsync(string email, string password);
        Task<AuthResult> RegisterWithJwt(string email, string password);
        Task<string> LogoutSessionAsync(string refreshToken);
        #endregion


        #region OTP 

        Task<AuthResult> SendOtpAsync(string email);
        Task<AuthResult> VerifyOtpAsync(string otp, string email);
        #endregion

    }
}
