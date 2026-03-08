using System.Security.Cryptography;
using System.Text;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Repository;
using UserAuth.Api.Interfaces.Service;

namespace UserAuth.Api.Services;


public class OtpService : IOtpService
{
    private readonly IOtpVerificationRepository _otpVerificationRepository;

    private readonly IEmailService _emailService;
    private readonly ILogger<OtpService> _logger;
    private readonly IUserRepository _userRepository;

    public OtpService(IOtpVerificationRepository otpVerificationRepository, IEmailService emailService, ILogger<OtpService> logger, IUserRepository userRepository)
    {
        this._otpVerificationRepository = otpVerificationRepository;
        _emailService = emailService;
        _logger = logger;
        this._userRepository = userRepository;
    }

    public async Task<bool> GenerateAndSendOtpAsync(string email)
    {
        email = email.ToLower().Trim();

        // only can sent 5 otp in 15 min 
        // currently stoping to sent 1 otp per min is remove .. 

        if (await IsRateLimited(email))
        {
            return false;
        }

        //revoke previous OTPs
        await _otpVerificationRepository.RevokePreviousOtpsAsync(email);

        // Generate 6-digit OTP 
        var otp = new Random().Next(100000, 1000000).ToString();
        var otphash = HashOtp(otp);

        var otpEntry = new OtpVerification
        {
            Email = email.ToLower().Trim(),
            OtpCode = otphash,
            ExpiryTime = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };
        await _otpVerificationRepository.AddAsync(otpEntry);

        _logger.LogInformation("OTP generated for {Email} : {Otp}", email, otp);

        await _emailService.SendEmailAsync(
           email,
           "Your OTP Code",
           $"Your OTP is: {otp}. It expires in 5 minutes."
       );

        return true;
    }

    public async Task<bool> VerifyOtpAsync(string email, string otp)
    {
        var otpHash = HashOtp(otp);

        var otpEntry = await _otpVerificationRepository.VerifyOtpAsync(email, otpHash);

        if (otpEntry == null)
        {
            _logger.LogWarning("Invalid OTP attempt for {Email}", email);
            return false;
        }
        _logger.LogInformation("OTP verified successfully for {Email} and added user too", email);

        await _otpVerificationRepository.MarkAsUsedAsync(email, otpHash);
        return true;
    }

    #region Helper Method
    private async Task<bool> IsRateLimited(string email)
    {
        email = email.ToLower().Trim();
        var fifteenMinutesAgo = DateTime.UtcNow.AddMinutes(-15);

        var count = await _otpVerificationRepository.CountNumberOfOtpAsync(email, fifteenMinutesAgo);
        return count >= 5;
    }

    /*  private async Task<bool> IsCooldownActive(string email)
      {
          await _otpVerificationRepository.

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
  */


    public string HashOtp(string otp)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(otp);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    #endregion



}