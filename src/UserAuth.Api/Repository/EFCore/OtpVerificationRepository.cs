using Microsoft.EntityFrameworkCore;
using UserAuth.Api.Data;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Repository;

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

        public async Task<OtpVerification?> GetByIdAsync(int id)
        {
            return await _dbContext.OtpVerifications
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<OtpVerification?> VerifyOtpAsync(string email, string otpCode)
        {
            return await _dbContext.OtpVerifications
                .Where(o => o.Email == email
                         && o.OtpCode == otpCode
                         && o.IsUsed == false
                         && o.ExpiryTime > DateTime.UtcNow)
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

        public async Task MarkAsUsedAsync(string email, string otp)
        {
            var result = await _dbContext.OtpVerifications.FirstOrDefaultAsync(x => x.Email == email && x.OtpCode == otp);
            if (result != null)
            {
                result.IsUsed = true;
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

        public async Task<bool> RevokePreviousOtpsAsync(string email)
        {
            var result = await _dbContext.OtpVerifications.
                 Where(x => x.Email == email
                 && !x.IsUsed &&
                 x.ExpiryTime > DateTime.UtcNow)
                 .ToListAsync();

            foreach (var item in result)
            {
                item.IsUsed = false;
            }
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> CountNumberOfOtpAsync(string email, DateTime dateTime)
        {
            int count = _dbContext.OtpVerifications
               .Count(x => x.Email == email
               && x.CreatedAt >= dateTime
               && !x.IsUsed);
            return count;
        }
    }
}