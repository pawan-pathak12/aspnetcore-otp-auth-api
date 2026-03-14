using System.Net;
using System.Net.Http.Json;
using UserAuth.Api.DTOs.Users;
using UserAuth.Api.Entities;
using UserAuthWithOTP.API.Fixtures;

namespace UserAuthWithOTP.API.Controller
{
    [TestClass]
    public class UserControllerTest : IntegrationTestBase
    {
        private string Password = "HelloTesting!1";

        //Authorizattion is used so sent token to pass

        #region Postive Path

        [TestMethod]
        public async Task CreateUser_WhenValid_Return201()
        {
            //Arrange
            var request = new User
            {
                Email = "testuser11111@gmail.com",
                Password = Password
            };

            //Act
            var resposne = await _client.PostAsJsonAsync("/api/User", request);

            //Assert
            Assert.AreEqual(HttpStatusCode.Created, resposne.StatusCode);
        }

        [TestMethod]
        public async Task GetUserById_WhenUserExists_Return200()
        {
            //Arrange
            var user = await testDataBuilder.CreateAndReturnUser();

            //Act
            var response = await _client.GetAsync($"/api/User/{user.Id}");

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task GetAllUsers_WhenUserExists_Return200()
        {
            //Arrange
            await testDataBuilder.CreateAndReturnUser();

            //Act
            var response = await _client.GetAsync("/api/User");

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task UpdateUser_WhenValid_Return204()
        {
            //Arrange
            var user = await testDataBuilder.CreateAndReturnUser();

            var request = new UpdateUserDto
            {
                Id = user.Id,
                Role = "User",
                IsActive = true,
                Password = Password
            };

            //Act
            var response = await _client.PutAsJsonAsync($"/api/User/{user.Id}", request);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteUser_WhenValid_Return204()
        {
            //Arrange
            var user = await testDataBuilder.CreateAndReturnUser();

            //Act
            var response = await _client.DeleteAsync($"/api/User/{user.Id}");

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        #endregion


        #region Negative Path

        [TestMethod]
        public async Task CreateUser_WhenInvalid_Return400()
        {
            //Arrange
            var request = new CreateUserDto
            {
                Email = "", //empty  email addrerss 
                Password = Password
            };

            //Act
            var resposne = await _client.PostAsJsonAsync("/api/User", request);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, resposne.StatusCode);
        }

        [TestMethod]
        public async Task GetUserById_WhenUserNotFound_Return404()
        {
            //Arrange
            var invalidId = new Random().Next(1000, 9999);

            //Act
            var response = await _client.GetAsync($"/api/User/{invalidId}");

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task UpdateUser_WhenInvalid_Return404()
        {
            //Arrange
            var user = await testDataBuilder.CreateAndReturnUser();

            var request = new UpdateUserDto
            {
                Id = new Random().Next(10000, 99999),   // pass random id 
                Role = "User",
                IsActive = true,
                Password = Password
            };

            //Act
            var response = await _client.PutAsJsonAsync($"/api/User/{request.Id}", request);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteUser_WhenUserNotFound_Return404()
        {
            //Arrange
            var invalidId = new Random().Next(1000, 9999);

            //Act
            var response = await _client.DeleteAsync($"/api/User/{invalidId}");

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion
    }
}