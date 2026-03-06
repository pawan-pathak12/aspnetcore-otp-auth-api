using UserAuth.Api.Data;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Repository;

namespace UserAuth.Api.Repository.InMemory
{
    public class InMemoryOtpVerificationRepository : IOtpVerificationRepository
    {
        private readonly InMemoryDbContext _dbContext;
        private readonly List<OtpVerification> _otpVerifications;

        public InMemoryOtpVerificationRepository(InMemoryDbContext dbContext)
        {
            _dbContext = dbContext;
            _otpVerifications = _dbContext.OtpVerifications ?? new List<OtpVerification>();
        }

        public Task AddAsync(OtpVerification otp)
        {
            otp.Id = _otpVerifications.Count > 0
                ? _otpVerifications.Max(o => o.Id) + 1
                : 1;

            otp.CreatedAt = DateTime.UtcNow;
            if (otp.ExpiryTime == default)
                otp.ExpiryTime = DateTime.UtcNow.AddMinutes(10);

            _otpVerifications.Add(otp);
            return Task.CompletedTask;
        }

        public Task<OtpVerification?> GetByEmailAsync(string email)
        {
            var otp = _otpVerifications.Find(x => x.Email == email && !x.IsUsed);
            return Task.FromResult(otp);
        }
        public Task<bool> AnyActiveOtpForEmailAsync(string email)
        {
            var hasActive = _otpVerifications.Any(o =>
                string.Equals(o.Email, email, StringComparison.OrdinalIgnoreCase) &&
                !o.IsUsed &&
                o.ExpiryTime > DateTime.UtcNow);

            return Task.FromResult(hasActive);
        }

        public Task<int> CountNumberOfOtpAsync(string email, DateTime dateTime)
        {
            var count = _otpVerifications.Count(o =>
                string.Equals(o.Email, email, StringComparison.OrdinalIgnoreCase) &&
                o.CreatedAt >= dateTime.Date &&
                o.CreatedAt < dateTime.Date.AddDays(1));

            return Task.FromResult(count);
        }

        public Task DeleteAllForEmailAsync(string email)
        {
            var toRemove = _otpVerifications
                .Where(o => string.Equals(o.Email, email, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var item in toRemove)
            {
                _otpVerifications.Remove(item);
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            var otp = _otpVerifications.FirstOrDefault(o => o.Id == id);
            if (otp != null)
            {
                _otpVerifications.Remove(otp);
            }

            return Task.CompletedTask;
        }

        public Task<OtpVerification?> GetByIdAsync(int id)
        {
            var otp = _otpVerifications.FirstOrDefault(o => o.Id == id);
            return Task.FromResult(otp);
        }

        public Task MarkAsUsedAsync(string email, string otpCode)
        {
            var otp = _otpVerifications
                .Where(o => string.Equals(o.Email, email, StringComparison.OrdinalIgnoreCase) &&
                            o.OtpCode == otpCode &&
                            !o.IsUsed &&
                            o.ExpiryTime > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();

            if (otp != null)
            {
                otp.IsUsed = true;
            }

            return Task.CompletedTask;
        }

        public Task<bool> RevokePreviousOtpsAsync(string email)
        {
            var previousOtps = _otpVerifications
                .Where(o => string.Equals(o.Email, email, StringComparison.OrdinalIgnoreCase) &&
                            !o.IsUsed &&
                            o.ExpiryTime > DateTime.UtcNow)
                .ToList();

            foreach (var otp in previousOtps)
            {
                otp.IsUsed = true;
            }

            return Task.FromResult(previousOtps.Any());
        }

        public Task UpdateAsync(OtpVerification otp)
        {
            var existing = _otpVerifications.FirstOrDefault(o => o.Id == otp.Id);
            if (existing == null)
            {
                return Task.CompletedTask;
            }

            // Update allowed fields

            existing.IsUsed = otp.IsUsed;

            return Task.CompletedTask;
        }

        public Task<OtpVerification?> VerifyOtpAsync(string email, string otpCode)
        {
            var validOtp = _otpVerifications
                .Where(o => string.Equals(o.Email, email, StringComparison.OrdinalIgnoreCase) &&
                            o.OtpCode == otpCode &&
                            !o.IsUsed &&
                            o.ExpiryTime > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();

            return Task.FromResult(validOtp);
        }
    }
}