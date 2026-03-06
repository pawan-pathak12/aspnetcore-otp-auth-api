using System.Security.Cryptography;
using System.Text;
using UserAuth.Api.Entities;
namespace UserAuthWithOTP.Unit.Services
{
    [TestClass]
    public class AuthServiceTests : AuthServiceTestBase
    {
        private string Passeord = "HelloWorld!";
        private Random rand = new Random();

        #region Positive Path 

        [TestMethod]
        public async Task SendOtpToRegisterAsync_WhenVaild_SendsOptToUser()
        {
            //it sends otp to provided email 

            //Arrange 

            var email = "testuser@gmail.com";

            //Act 
            var result = await _authService.SendOtpToRegisterAsync(email);

            //Assert
            Assert.IsTrue(result.IsSuccess);

        }
        [TestMethod]
        public async Task VerifyOtpAsync_WhenValid_ReturnTrue()
        {
            //it takes email that is entered while sending otp and otp that is sended to that email
            //Arrange 
            string email = $"testuser{rand.Next(1000, 9999)}@gmail.com";

            await _authService.SendOtpToRegisterAsync(email);

            var storedOtp = await _otpVerificationRepository.GetByEmailAsync(email);

            Assert.IsNotNull(storedOtp);

            var otpCode = storedOtp.OtpCode;

            //Act 

            var result = await _authService.VerifyOtpAsync(otpCode, email);

            //Assert
            Assert.IsTrue(result.IsSuccess);

        }

        [TestMethod]
        public async Task RegisterAsync_WhenValid_ReturnTrue()
        {
            //Arrange 
            var rand = new Random();
            var user = new User
            {
                Email = $"testuser{rand.Next(1000, 9999)}@gmail.com",
                IsActive = true,
                IsVerified = true,
                Role = "User",
                Password = Passeord
            };
            //Act 
            var result = await _authService.RegisterAsync(user);

            //Assert
            Assert.IsTrue(result.IsSuccess);
            var storedUser = await _userRepo.GetByEmailAsync(user.Email);
            Assert.IsNotNull(storedUser);

        }

        [TestMethod]
        public async Task LoginAsync_WhenValid_ReturnAccessAndRefreshToken()
        {
            //Arrange 
            var rand = new Random();

            //directing inserting to memory so hashing is required as service check it while validing user  during login

            // password hashing method is returning null

            var hashPassword = Hash(Passeord);
            var user = new User
            {
                Email = $"testuser{rand.Next(1000, 9999)}@gmail.com",
                IsActive = true,
                IsVerified = true,
                Role = "User",
                Password = hashPassword
            };

            await _userRepo.AddAsync(user);
            //Act 
            var result = await _authService.LoginAsync(user);

            //Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.AccessTokenhash);
            Assert.IsNull(result.RefreshToken);

        }

        [TestMethod]
        public async Task RotateRefreshTokenAsync_WhenTokenExists_ReturnNewAccessAndRefreshToken()
        {
            //Arrange 


            //Act 

            //Assert

        }

        [TestMethod]
        public async Task LogoutSessionAsync_WhenRefreshTokenValid_LogoutUser()
        {
            //Arrange 

            //Act 

            //Assert

        }

        #endregion


        #region Negative Path
        [TestMethod]
        public async Task LoginAsync_WhenInValid_ReturnFalse()
        {
            //Arrange 

            //Act 

            //Assert

        }
        [TestMethod]
        public async Task LogoutSessionAsync_WhenRefreshTokenInValid_ReturnFalse()
        {
            //Arrange 

            //Act 

            //Assert

        }

        [TestMethod]
        public async Task RotateRefreshTokenAsync_WhenTokenNotFound_Returnfalse()
        {
            //Arrange 

            //Act 

            //Assert

        }

        [TestMethod]
        public async Task RotateRefreshTokenAsync_WhenRevokedTokenUsed_Returnfalse()
        {
            //Arrange 

            //Act 

            //Assert

        }

        [TestMethod]
        public async Task RotateRefreshTokenAsync_WhenTokenIsExpired_Returnfalse()
        {
            //Arrange 

            //Act 

            //Assert

        }

        [TestMethod]
        public async Task SendOtpToRegisterAsync_WhenUserExists_ReturnFalse()
        {
            //Arrange 
            var user = new User
            {
                IsActive = true,
                IsVerified = true,
                Email = $"testuser{rand.Next(1000, 9999)}@gmailcom",
                Password = Passeord
            };

            await _userRepo.AddAsync(user);

            //Act 
            var result = await _authService.SendOtpToRegisterAsync(user.Email);

            //Assert
            Assert.IsFalse(result.IsSuccess);

        }

        [TestMethod]
        public async Task SendOtpToRegisterAsync_WhenEmailIsNull_ReturnFalse()
        {
            //Arrange 
            var email = "";

            //Act
            var result = await _authService.SendOtpToRegisterAsync(email);

            //Assert
            Assert.IsFalse(result.IsSuccess);

        }

        [TestMethod]
        public async Task VerifyOtpAsync_WhenWrongOtp_ReturnFalse()
        {
            //Arrange 
            var email = $"testuser{rand.Next(1000, 9999)}@gmailcom";

            string code = rand.Next(10000, 99999).ToString();
            var codeHash = Hash(code);
            var otp = new OtpVerification
            {
                Email = email,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsUsed = true,
                OtpCode = codeHash
            };
            await _otpVerificationRepository.AddAsync(otp);

            string wrongOtp = "11111";
            //Act 
            var result = await _authService.VerifyOtpAsync(wrongOtp, email); // password code

            //Assert
            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public async Task VerifyOtpAsync_WhenEmailNotFound_ReturnFalse()
        {
            //Arrange 

            var email = $"testuser{rand.Next(1000, 9999)}@gmailcom";
            string code = "11111";

            //Act 
            var result = await _authService.VerifyOtpAsync(code, email);

            //Assert
            Assert.IsFalse(result.IsSuccess);

        }
        [TestMethod]
        public async Task RegisterAsync_WhenAlreadyUserExists_Returnfalse()
        {
            //Arrange 

            var rand = new Random();
            var user = new User
            {
                Email = $"testuser{rand.Next(1000, 9999)}@gmail.com",
                IsActive = true,
                IsVerified = true,
                Role = "User",
                Password = Passeord
            };

            await _userRepo.AddAsync(user);

            //Act 
            var result = await _authService.RegisterAsync(user);

            //Assert
            Assert.IsFalse(result.IsSuccess);


        }
        #endregion

        private string Hash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);

        }


    }
}
