using UserAuth.Api.Entities;

namespace UserAuth.Api.Interfaces.Repository
{
    public interface IOtpVerificationRepository
    {
        Task AddAsync(OtpVerification otp);

        Task<OtpVerification?> GetByEmailAsync(string email);

        Task<OtpVerification?> GetByIdAsync(int id);
        Task<OtpVerification?> VerifyOtpAsync(string email, string otpCode);
        Task<bool> AnyActiveOtpForEmailAsync(string email);
        Task<int> CountNumberOfOtpAsync(string email, DateTime dateTime);
        Task MarkAsUsedAsync(string email, string otp);
        Task<bool> RevokePreviousOtpsAsync(string email);
        Task UpdateAsync(OtpVerification otp);

        Task DeleteAsync(int id);
        Task DeleteAllForEmailAsync(string email);
    }
}
