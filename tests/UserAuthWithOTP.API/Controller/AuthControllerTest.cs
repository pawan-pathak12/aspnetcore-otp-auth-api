using Microsoft.AspNetCore.Identity.Data;
using System.Net;
using System.Net.Http.Json;
using UserAuth.Api.DTOs;
using UserAuthWithOTP.API.Fixtures;

namespace UserAuthWithOTP.API.Controller
{
    [TestClass]
    public class AuthControllerTest : IntegrationTestBase
    {
        private static Random rand = new Random();
        private string Email = $"testuser@gmail{rand.Next(1000, 9999)}.com";

        #region Postive Path 
        [TestMethod]
        public async Task RegisterAsync_WhenValid_Return200()
        {
            //Arrange 

            //Act 

            //Assert

        }

        [TestMethod]
        public async Task VerifyAndRegisterAsync_WhenValid_Return200()
        {
            //Arrange 

            var otpData = await testDataBuilder.CreateAndReturnOtpData();
            var request = new VerifyOtpRequestDto
            {
                Email = otpData.Email,
                Password = testDataBuilder.Password,
                Otp = otpData.OtpCode
            };

            //Act 

            var response = await _client.PostAsJsonAsync("/api/Auth/register-verify-create-user", request);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        }

        [TestMethod]
        public async Task LoginAsync_WhenValid_Return200WithAccessTokenAndSetRefreshCookie()
        {
            //Arrange 
            var user = await testDataBuilder.CreateAndReturnUser();

            var request = new LoginRequestDto
            {
                Email = user.Email,
                Password = testDataBuilder.Password
            };

            //Act 
            var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);


        }

        [TestMethod]
        public async Task Refresh_WhenTokenExists_Return200WithNewAccesTokenAndSetRefreshCookie()
        {
            //Arrange 
            var user = await testDataBuilder.CreateAndReturnUser();

            var loginRequest = new LoginRequest
            {
                Email = user.Email,
                Password = testDataBuilder.Password
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
            Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

            //Act 
            var response = await _client.PostAsync("/api/Auth/refresh", null);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result.AccessToken));
            // Verify new refresh token cookie was set
            Assert.IsTrue(
                response.Headers.Contains("Set-Cookie"),
                "Response should set a new refresh token cookie"
            );
        }

        [TestMethod]
        public async Task Logout_WhenLoginIn_Return200AndClearsRefreshCookie()
        {

            //Act 
            var response = await _client.PostAsJsonAsync("/api/Auth/Logout", "");

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        }

        #endregion

        #region Negative Path  
        [TestMethod]
        public async Task RegisterAsync_WhenInValid_Return400()
        {
            //Arrange 

            var user = await testDataBuilder.CreateAndReturnUser();
            var request = new SentOtpDto
            {
                Email = user.Email
            };

            //Act 
            var response = await _client.PostAsJsonAsync("/api/Auth/register-sent-otp", request);

            //Assert

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

        }

        [TestMethod]
        public async Task VerifyAndRegisterAsync_WhenInValid_Return400()
        {
            //Arrange 
            var request = new VerifyOtpRequestDto
            {
                Email = Email
            };

            //Act 
            var response = await _client.PostAsJsonAsync("/api/Auth/register-verify-create-user", request);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

        }

        [TestMethod]
        public async Task LoginAsync_WhenInvalid_Return404()
        {
            //Arrange 
            var request = new LoginRequestDto
            {
                Email = Email,    // pass unexits email
                Password = testDataBuilder.Password
            };

            //Act 
            var response = await _client.PostAsJsonAsync("/api/login", request);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Refresh_WhenTokenNotFound_Return401()
        {
            //Act 
            var response = await _client.PostAsJsonAsync("/api/Auth/refresh", "");
            //Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

        }


        #endregion
    }
}
