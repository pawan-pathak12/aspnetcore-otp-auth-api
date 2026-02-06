using UserAuth.Api.DTOs;

namespace UserAuth.Api.Interfaces.Service
{
    public interface IAuthService
    {

        #region JWT

        Task<(bool success, string errorMessage, AuthResponseDto? response)> RotateRefreshTokenAsync(string refreshToken);
        Task<(bool success, string error, AuthResponseDto? response)> LoginWithJwtAsync(string email, string password);
        Task<(bool success, string errorMessage, int id)> RegisterWithJwt(string email, string password);
        Task<string> LogoutSessionAsync(string refreshToken);
        #endregion


        #region OTP 

        Task<(bool success, string ErrorMessage)> SendOtpAsync(string email);
        Task<(bool success, string errorMessage)> VerifyOtpAsync(string otp, string email);
        #endregion

    }
}
