using Microsoft.EntityFrameworkCore;
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

    public async Task GenerateAndSaveOtpAsync(string email)
    {
        // Generate 6-digit OTP (fixed the range!)
        var otp = new Random().Next(100000, 1000000).ToString();

        var otpEntry = new OtpVerification
        {
            Email = email.ToLower().Trim(),
            OtpCode = otp,
            ExpiryTime = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        };

        _dbContext.OtpVerifications.Add(otpEntry);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("OTP generated for {Email} : {Otp}", email, otp);

        await _emailService.SendEmailAsync(
            email,
            "Your OTP Code",
            $"Your OTP is: {otp} \n It expires in 5 minutes.If you didn't request this, please ignore."
        );
    }

    public async Task<bool> VerifyOtpAndCreateUserAsync(string email, string otp)
    {
        var otpEntry = await _dbContext.OtpVerifications
            .Where(x => x.Email == email &&
                        x.OtpCode == otp &&
                        !x.IsUsed &&
                        x.ExpiryTime > DateTime.UtcNow)
            .OrderByDescending(x => x.Id)
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
}