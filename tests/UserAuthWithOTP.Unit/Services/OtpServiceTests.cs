using System.Security.Cryptography;
using System.Text;
using UserAuth.Api.Entities;
using UserAuthWithOTP.Unit.Common;

namespace UserAuthWithOTP.Unit.Services
{
    [TestClass]
    public class OtpServiceTests : OtpServiceTestBase
    {
        private Random rand = new Random();


        #region Postive Path 

        [TestMethod]
        public async Task GenerateAndSendOtpAsync_WhenValid_ReturnTrue()
        {
            //Arrange 
            string email = $"test@gmail{rand.Next(1000, 9999)}.com";

            //Act 
            var result = await _otpService.GenerateAndSendOtpAsync(email);

            //Assert
            Assert.IsTrue(result);

        }

        [TestMethod]
        public async Task VerifyOtpAsync_WhenValid_ReturnTrue()
        {
            //Arrange 
            string email = $"test@gmail{rand.Next(1000, 9999)}.com";
            var otpCode = rand.Next(100000, 999999).ToString();
            var otp = new OtpVerification
            {
                Email = email,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                OtpCode = HashOtp(otpCode),
                CreatedAt = DateTime.UtcNow
            };

            await _otpVerificationRepository.AddAsync(otp);

            //Act 
            var result = await _otpService.VerifyOtpAsync(otp.Email, otpCode);

            //Assert
            Assert.IsTrue(result);

        }

        #endregion

        #region Negative Path 

        [TestMethod]
        public async Task GenerateAndSendOtpAsync_WhenManyOtpSended_ReturnFalse()
        {
            //Arrange 
            string email = $"test@gmail{rand.Next(1000, 9999)}.com";

            for (int i = 1; i <= 5; i++)
            {
                await _otpService.GenerateAndSendOtpAsync(email);
            }

            //Act 
            var result = await _otpService.GenerateAndSendOtpAsync(email);

            //Assert
            Assert.IsFalse(result);

        }

        [TestMethod]
        public async Task VerifyOtpAsync_WhenInvalidOtp_ReturnFalse()
        {
            //Arrange 

            var otpCode = rand.Next(100000, 999999).ToString();
            string email = $"test@gmail{rand.Next(1000, 9999)}.com";

            //Act 
            var result = await _otpService.VerifyOtpAsync(email, otpCode);

            //Assert
            Assert.IsFalse(result);

        }

        #endregion

        public string HashOtp(string otp)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(otp);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
