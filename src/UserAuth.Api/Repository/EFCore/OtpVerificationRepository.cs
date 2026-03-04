using Microsoft.EntityFrameworkCore;
using UserAuth.Api.Data;
using UserAuth.Api.Entities;

namespace UserAuth.Api.Repository.EFCore
{
    public class OtpVerificationRepository : IOtpVerificationRepository
    {
        private readonly AppDbContext _dbContext;

        public OtpVerificationRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(OtpVerification otp)
        {
            await _dbContext.OtpVerifications.AddAsync(otp);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<OtpVerification> AddAndReturnAsync(OtpVerification otp)
        {
            await _dbContext.OtpVerifications.AddAsync(otp);
            await _dbContext.SaveChangesAsync();
            return otp;
        }


        public async Task<OtpVerification?> GetByIdAsync(int id)
        {
            return await _dbContext.OtpVerifications
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<OtpVerification?> GetLatestByEmailAsync(string email)
        {
            return await _dbContext.OtpVerifications
                .Where(o => o.Email == email)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<OtpVerification?> GetValidOtpByEmailAsync(string email, string otpCode)
        {
            return await _dbContext.OtpVerifications
                .Where(o => o.Email == email
                         && o.OtpCode == otpCode
                         && o.IsUsed == false
                         && o.ExpiryTime > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> AnyActiveOtpForEmailAsync(string email)
        {
            return await _dbContext.OtpVerifications
                .AnyAsync(o => o.Email == email
                            && o.IsUsed == false
                            && o.ExpiryTime > DateTime.UtcNow);
        }


        public async Task UpdateAsync(OtpVerification otp)
        {
            _dbContext.OtpVerifications.Update(otp);
            await _dbContext.SaveChangesAsync();
        }

        public async Task MarkAsUsedAsync(int otpId)
        {
            var otp = await _dbContext.OtpVerifications.FindAsync(otpId);
            if (otp != null)
            {
                otp.IsUsed = true;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task MarkAsUsedByEmailAndCodeAsync(string email, string code)
        {
            var otp = await GetValidOtpByEmailAsync(email, code);
            if (otp != null)
            {
                otp.IsUsed = true;
                await _dbContext.SaveChangesAsync();
            }
        }


        public async Task DeleteAsync(int id)
        {
            var otp = await _dbContext.OtpVerifications.FindAsync(id);
            if (otp != null)
            {
                _dbContext.OtpVerifications.Remove(otp);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteExpiredOtpsAsync()
        {
            var expired = await _dbContext.OtpVerifications
                .Where(o => o.ExpiryTime < DateTime.UtcNow)
                .ToListAsync();

            if (expired.Any())
            {
                _dbContext.OtpVerifications.RemoveRange(expired);
                await _dbContext.SaveChangesAsync();
            }
        }


        // delete all opts at once 

        public async Task DeleteAllForEmailAsync(string email)
        {
            var otps = await _dbContext.OtpVerifications
                .Where(o => o.Email == email)
                .ToListAsync();

            if (otps.Any())
            {
                _dbContext.OtpVerifications.RemoveRange(otps);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}