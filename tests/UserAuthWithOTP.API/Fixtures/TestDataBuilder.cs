using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Repository;
using UserAuth.Api.Interfaces.Service;

namespace UserAuthWithOTP.API.Fixtures
{
    public class TestDataBuilder
    {
        public IUserRepository _userRepository;
        public IOtpVerificationRepository _otpVerificationRepository;
        public IRefreshTokenRepository _refreshTokenRepository;
        public ITokenService _tokenService;
        public string Password = "TestUser!11";
        private readonly PasswordHasher<User> _passwordHasher;


        public TestDataBuilder(IServiceProvider serviceProvider)
        {
            _passwordHasher = new PasswordHasher<User>();
            _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
            _otpVerificationRepository = serviceProvider.GetRequiredService<IOtpVerificationRepository>();
            _refreshTokenRepository = serviceProvider.GetRequiredService<IRefreshTokenRepository>();
            _tokenService = serviceProvider.GetRequiredService<ITokenService>();
        }

        private static string Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task<User> CreateAndReturnUser()
        {
            var rand = new Random();

            var user = new User
            {
                Email = $"user+{rand.Next(10000, 99999)}@gmail.com",
                IsActive = true,
                IsVerified = true,
                Password = Password,
                Role = "Admin",
                CreateAt = DateTime.UtcNow
            };
            var passwordhash = HashPassword(user);
            user.Password = passwordhash;

            var userId = await _userRepository.AddAsync(user);
            var userData = await _userRepository.GetByIdAsync(userId);

            return new User
            {
                Email = userData.Email,
                Role = userData.Role,
                Id = userId,
                Password = Password,
                IsActive = true,
                IsVerified = true
            };
        }

        public async Task<OtpVerification> CreateAndReturnOtpData()
        {
            var rand = new Random();

            int otpCode = rand.Next(100000, 999999);
            string otpString = otpCode.ToString();
            var otp = new OtpVerification
            {
                Email = $"testuser@gmail{rand.Next(1000, 9999)}.com",
                OtpCode = Hash(otpString),
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };
            await _otpVerificationRepository.AddAsync(otp);
            var result = await _otpVerificationRepository.GetByEmailAsync(otp.Email);
            if (result == null)
            {
                return null;
            }

            result.OtpCode = $"{otpCode}";   // returning otp 
            return result;

        }

        public async Task<RefreshToken?> CreateAndReturnRefreshToken()
        {

            var user = await CreateAndReturnUser();

            var reftoken = _tokenService.GenerateRefreshTokenAsync();

            var refreshToken = new RefreshToken
            {
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = user.Id,
                TokenHash = _tokenService.HashToken(reftoken)
            };

            var refId = await _refreshTokenRepository.AddAsync(refreshToken);

            var tokenData = await _refreshTokenRepository.GetByIdAsync(refId);

            tokenData?.TokenHash = reftoken;
            return tokenData;

        }


        #region Helper 

        public string HashPassword(User user)
        {
            return _passwordHasher.HashPassword(user, user.Password);
        }
        #endregion

    }
}
