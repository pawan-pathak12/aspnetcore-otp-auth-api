using System.Security.Cryptography;
using System.Text;

namespace UserAuthWithOTP.Unit.Services
{
    [TestClass]
    public class AuthServiceTests : AuthServiceTestBase
    {

        #region Positive Path 

        [TestMethod]
        public async Task SendOtpToRegisterAsync_WhenVaild_SendsOptToUser()
        {
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

            //Act 
            var result = await _authService.VerifyOtpAsync();

            //Assert

        }

        [TestMethod]
        public async Task RegisterAsync_WhenValid_ReturnTrue()
        {
            //Arrange 

            //Act 

            //Assert

        }


        [TestMethod]
        public async Task LoginAsync_WhenValid_ReturnAccessAndRefreshToken()
        {
            //Arrange 

            //Act 

            //Assert

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

            //Act 

            //Assert

        }
        [TestMethod]
        public async Task VerifyOtpAsync_WhenInValid_ReturnFalse()
        {
            //Arrange 

            //Act 

            //Assert

        }
        [TestMethod]
        public async Task RegisterAsync_WhenAlreadyUserExists_Returnfalse()
        {
            //Arrange 

            //Act 

            //Assert

        }
        #endregion

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return string.Empty;

        }
    }
}
