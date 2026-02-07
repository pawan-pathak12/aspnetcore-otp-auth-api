namespace UserAuth.Api.Interfaces.Service
{
    public interface IOtpService
    {
        Task<bool> GenerateAndSaveOtpAsync(string email);
        Task<bool> VerifyOtpAndCreateUserAsync(string email, string otp);
        Task<bool> VerifyOtpAsync(string email, string otp);
    }
}
