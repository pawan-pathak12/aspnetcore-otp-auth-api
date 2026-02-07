using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using UserAuth.Api.Data;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Service;

namespace UserAuth.Api.Services;


public class OtpService : IOtpService
{
    private readonly AppDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly ILogger<OtpService> _logger;

    public OtpService(AppDbContext dbContext, IEmailService emailService, ILogger<OtpService> logger)
    {
        _dbContext = dbContext;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> GenerateAndSaveOtpAsync(string email)
    {
        email = email.ToLower().Trim();

        if (IsCooldownActive(email))
        {
            return false;
        }
        if (IsRateLimited(email))
        {
            return false;
        }

        //revoke previous OTPs
        await RevokePreviousOtpsAsync(email);

        // Generate 6-digit OTP (fixed the range!)
        var otp = new Random().Next(100000, 1000000).ToString();
        var otphash = HashOtp(otp);
        Console.WriteLine($"otp hash is {otphash}");

        var otpEntry = new OtpVerification
        {
            Email = email.ToLower().Trim(),
            OtpCode = otphash,
            ExpiryTime = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.OtpVerifications.Add(otpEntry);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("OTP generated for {Email} : {Otp}", email, otp);

        await _emailService.SendEmailAsync(
           email,
           "Your OTP Code",
           $"Your OTP is: {otp}. It expires in 5 minutes."
       );

        return true;
    }

    public async Task<bool> VerifyOtpAndCreateUserAsync(string email, string otp)
    {
        var otpHash = HashOtp(otp);

        var otpEntry = await _dbContext.OtpVerifications
            .Where(x => x.Email == email &&
                        x.OtpCode == otpHash &&
                        !x.IsUsed &&
                        x.ExpiryTime > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (otpEntry == null)
        {
            _logger.LogWarning("Invalid OTP attempt for {Email}", email);
            return false;
        }
        otpEntry.IsUsed = true;

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
        {
            user = new User
            {
                Email = email,
                IsActive = true,
                IsVerified = true,
                CreateAt = DateTime.UtcNow
            };
            await _dbContext.AddAsync(user);

        }
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("OTP verified successfully for {Email} and added user too", email);
        return true;
    }

    public async Task<bool> VerifyOtpAsync(string email, string otp)
    {
        var otpHash = HashOtp(otp);

        var otpEntry = await _dbContext.OtpVerifications
            .Where(x => x.Email == email &&
                        x.OtpCode == otpHash &&
                        !x.IsUsed &&
                        x.ExpiryTime > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (otpEntry == null)
        {
            _logger.LogWarning("Invalid OTP attempt for {Email}", email);
            return false;
        }
        otpEntry.IsUsed = true;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    #region Helper Method
    private bool IsRateLimited(string email)
    {
        email = email.ToLower().Trim();
        var fifteenMinutesAgo = DateTime.UtcNow.AddMinutes(-15);
        int count = _dbContext.OtpVerifications.Count(x => x.Email == email
            && x.CreatedAt >= fifteenMinutesAgo &&
            !x.IsUsed);

        return count >= 5;
    }

    private bool IsCooldownActive(string email)
    {
        var lastOtp = _dbContext.OtpVerifications.Where(
            x => x.Email == email)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        if (lastOtp == null)
        {
            return false;
        }

        return (DateTime.UtcNow - lastOtp.CreatedAt).TotalSeconds < 60;
    }

    private async Task RevokePreviousOtpsAsync(string email)
    {
        var activeOtps = await _dbContext.OtpVerifications
            .Where(x => x.Email == email && !x.IsUsed)
            .ToListAsync();

        foreach (var otp in activeOtps)
        {
            otp.IsUsed = true;
        }
        await _dbContext.SaveChangesAsync();
    }

    public string HashOtp(string otp)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(otp);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    #endregion



}