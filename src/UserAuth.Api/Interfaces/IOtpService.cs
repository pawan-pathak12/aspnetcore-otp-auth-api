namespace UserAuth.Api.Interfaces
{
    public interface IOtpService
    {
        Task GenerateAndSaveOtpAsync(string email);
        Task<bool> VerifyOtpAsync(string email, string otp);
    }
}
