using System.Security.Cryptography;
using System.Text;
using UserAuth.Api.Entities;
namespace UserAuthWithOTP.Unit.Services
{
    [TestClass]
    public class AuthServiceTests : AuthServiceTestBase
    {
        private string Password = "HelloWorld!";
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
            //Arrange 
            string email = $"testuser{rand.Next(1000, 9999)}@gmail.com";
            var otp = GenerateOtp();

            var otpVerification = new OtpVerification
            {
                Email = email,
                OtpCode = Hash(otp),
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _otpVerificationRepository.AddAsync(otpVerification);

            //Act 

            var result = await _authService.VerifyOtpAsync(otp, otpVerification.Email);

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
                Password = Password
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

            //directing inserting to memory so hashing is required as service check it while validing user during login

            var user = new User
            {
                Email = $"testuser{rand.Next(1000, 9999)}@gmail.com",
                IsActive = true,
                IsVerified = true,
                Role = "User",
                Password = Password
            };

            var hashPassword = HashPassword(user);
            user.Password = hashPassword;
            await _userRepo.AddAsync(user);

            var login = new User
            {
                Email = user.Email,
                Role = user.Role,
                Password = Password
            };
            //Act 
            var result = await _authService.LoginAsync(login);

            //Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.AccessTokenhash);
            Assert.IsNotNull(result.RefreshToken);

        }

        [TestMethod]
        public async Task RotateRefreshTokenAsync_WhenTokenExists_ReturnNewAccessAndRefreshToken()
        {
            //Arrange 
            var user = new User
            {
                Email = $"testuser{rand.Next(1000, 9999)}@gmail.com",
                IsActive = true,
                IsVerified = true,
                Role = "User",
                Password = Password
            };

            var hashPassword = HashPassword(user);
            user.Password = hashPassword;

            var userId = await _userRepo.AddAsync(user);

            var token = _tokenService.GenerateRefreshTokenAsync();

            var refToken = new RefreshToken
            {
                TokenHash = Hash(token),
                IsRevoked = false,
                UserId = userId,
                ExpiredAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                User = user
            };
            await _inMemoryRefreshToken.AddAsync(refToken);

            //Act 

            var result = await _authService.RotateRefreshTokenAsync(token);

            //Assert

            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.AccessTokenhash);
            Assert.IsNotNull(result.RefreshToken);

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
        public async Task LoginAsync_WhenUssrNotFound_ReturnFalse()
        {
            //Arrange 
            var user = new User
            {
                Email = $"testuser{rand.Next(1000, 9999)}.com",
                IsActive = true,
                Password = Password,
                IsVerified = true
            };

            //Act 
            var result = await _authService.LoginAsync(user);

            //Assert
            Assert.IsFalse(result.IsSuccess);

        }

        [TestMethod]
        public async Task LoginAsync_WhenWrongPassword_ReturnFalse()
        {

            //Arrange 
            var hashPassword = Hash(Password);
            var user = new User
            {
                Email = $"testuser{rand.Next(1000, 9999)}.com",
                IsActive = true,
                Password = hashPassword,
                IsVerified = true,
                CreateAt = DateTime.UtcNow,
                Role = "User"
            };

            await _userRepo.AddAsync(user);

            var login = new User
            {
                Email = user.Email,
                Password = "Testing",
                Role = user.Role
            };
            //Act 
            var result = await _authService.LoginAsync(login);

            //Assert
            Assert.IsFalse(result.IsSuccess);

        }
        [TestMethod]
        public async Task LoginAsync_WhenUserIsInActive_ReturnFalse()
        {
            //Arrange 
            var hashPassword = Hash(Password);
            var user = new User
            {
                Email = $"testuser{rand.Next(1000, 9999)}.com",
                Password = hashPassword,
                CreateAt = DateTime.UtcNow,
                Role = "User"
            };

            await _userRepo.AddAsync(user);

            var login = new User
            {
                Email = user.Email,
                Password = Password,
                Role = user.Role
            };
            //Act 
            var result = await _authService.LoginAsync(login);

            //Assert
            Assert.IsFalse(result.IsSuccess);

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
            //Act 

            var result = await _authService.RotateRefreshTokenAsync("token");

            //Assert

            Assert.IsFalse(result.IsSuccess);

        }

        [TestMethod]
        public async Task RotateRefreshTokenAsync_WhenRevokedTokenUsed_Returnfalse()
        {
            //Arrange 
            var hashPassword = Hash(Password);
            var user = new User
            {
                Email = $"testuser{rand.Next(1000, 9999)}@gmail.com",
                IsActive = true,
                IsVerified = true,
                Role = "User",
                Password = hashPassword
            };

            var userId = await _userRepo.AddAsync(user);
            var token = _tokenService.GenerateRefreshTokenAsync();


            var refToken = new RefreshToken
            {
                TokenHash = Hash(token),
                IsRevoked = true,
                UserId = userId,
                ExpiredAt = DateTime.UtcNow.AddDays(7)
            };

            await _inMemoryRefreshToken.AddAsync(refToken);

            //Act 

            var result = await _authService.RotateRefreshTokenAsync(token);

            //Assert

            Assert.IsFalse(result.IsSuccess);



        }

        [TestMethod]
        public async Task RotateRefreshTokenAsync_WhenTokenIsExpired_Returnfalse()
        {
            //Arrange 
            var hashPassword = Hash(Password);
            var user = new User
            {
                Email = $"testuser{rand.Next(1000, 9999)}@gmail.com",
                IsActive = true,
                IsVerified = true,
                Role = "User",
                Password = hashPassword
            };

            var userId = await _userRepo.AddAsync(user);
            var token = _tokenService.GenerateRefreshTokenAsync();


            var refToken = new RefreshToken
            {
                TokenHash = Hash(token),
                IsRevoked = true,
                UserId = userId,
                ExpiredAt = DateTime.UtcNow.AddDays(-8)
            };

            await _inMemoryRefreshToken.AddAsync(refToken);

            //Act 

            var result = await _authService.RotateRefreshTokenAsync(token);

            //Assert

            Assert.IsFalse(result.IsSuccess);

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
                Password = Password
            };

            await _userRepo.AddAsync(user);

            //Act 
            var result = await _authService.SendOtpToRegisterAsync(user.Email);

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
                Password = Password
            };

            await _userRepo.AddAsync(user);

            //Act 
            var result = await _authService.RegisterAsync(user);

            //Assert
            Assert.IsFalse(result.IsSuccess);


        }
        #endregion

        private string Hash(string code)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(code);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);

        }

        private string HashPassword(User user)
        {
            return _passwordHasher.HashPassword(user, user.Password);

        }

        private string GenerateOtp()
        {
            return rand.Next(100000, 1000000).ToString();
        }
    }
}
