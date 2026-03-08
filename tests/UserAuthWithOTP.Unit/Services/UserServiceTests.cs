using UserAuth.Api.Data;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Service;
using UserAuth.Api.Repository.InMemory;
using UserAuth.Api.Services;
namespace UserAuthWithOTP.Unit.Services
{
    [TestClass]
    public class UserServiceTests
    {
        private IUserService _userService = null!;
        private InMemoryUserRepo _userRepo = null!;
        private Random rand = new Random();

        private string Password = "HelloUser!";
        [TestInitialize]
        public void TestInit()
        {
            var dbContext = new InMemoryDbContext();
            _userRepo = new InMemoryUserRepo(dbContext);

            _userService = new UserService(_userRepo);
        }

        #region Positive Path

        [TestMethod]
        public async Task AddAsync_WhenValid_ReturnTrue()
        {
            //Arrange
            var user = ReturnUser();

            //Act
            var result = await _userService.CreateAsync(user);

            //Assert
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public async Task GetByIdAsync_WhenUserExists_ReturnUser()
        {
            //Arrange
            var user = await CreateAndReturnUser();

            //Act

            var result = await _userService.GetByIdAsync(user.Id);
            //Assert
            Assert.IsNotNull(result);

        }

        [TestMethod]
        public async Task GetAllAsync_WhenUsersExist_ReturnUsers()
        {
            //Arrange
            var user = await CreateAndReturnUser();

            //Act
            var result = await _userService.GetAllAsync();

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task UpdateAsync_WhenUserExists_ReturnTrue()
        {
            //Arrange
            var user = await CreateAndReturnUser();

            var update = new User
            {
                Id = user.Id,
                Email = user.Email,
                IsActive = true,
                IsVerified = true,
                Role = "Admin",
                Password = user.Password
            };

            //Act
            var result = await _userService.UpdateAsync(update);

            //Assert
            Assert.IsTrue(result);
            var userData = await _userRepo.GetByIdAsync(user.Id);
            Assert.IsNotNull(userData);
            Assert.AreEqual(update.Role, userData.Role);
        }

        [TestMethod]
        public async Task DeleteAsync_WhenUserExist_ReturnTrue()
        {
            //Arrange
            var user = await CreateAndReturnUser();

            //Act
            var result = await _userService.DeleteAsync(user.Id);

            //Assert
            Assert.IsTrue(result);

        }

        [TestMethod]
        public async Task GetByEmailAsync_WhenUserExistsOfEnterEmail_ReturnUser()
        {
            //Arrange
            var user = await CreateAndReturnUser();

            //Act
            var result = await _userService.GetByEmailAsync(user.Email);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.Password, result.Password);
        }

        #endregion


        #region Negative Path

        [TestMethod]
        public async Task GetByIdAsync_WhenUserNotFound_ReturnFalse()
        {
            //Arrange
            var userId = 123873;
            //Act

            var result = await _userService.GetByIdAsync(userId);
            //Assert
            Assert.IsNull(result);

        }

        [TestMethod]
        public async Task GetAllAsync_WhenUsersNotFound_ReturnNull()
        {

            //Act
            var result = await _userService.GetAllAsync();

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task UpdateAsync_WhenUserNotFound_ReturnFalse()
        {
            //Arrange
            var user = new User
            {
                Email = $"testuser{rand.Next(1000, 9999)}@gmail.com",
                Password = Password,
                IsActive = true,
                Role = "User",
                IsVerified = true
            };

            var update = new User
            {
                Id = user.Id,
                Email = user.Email,
                IsActive = true,
                IsVerified = true,
                Role = "Admin",
                Password = user.Password
            };

            //Act
            var result = await _userService.UpdateAsync(update);

            //Assert
            Assert.IsFalse(result);

        }

        [TestMethod]
        public async Task DeleteAsync_WhenUserNotFound_ReturnFalse()
        {
            //A Arrange 
            int id = rand.Next(1000, 9999);

            //Act
            var result = await _userService.DeleteAsync(id);

            //Assert
            Assert.IsFalse(result);

        }

        [TestMethod]
        public async Task GetByEmailAsync_WhenUserNotFound_ReturnNull()
        {
            //Arrange
            var email = $"test{rand.Next(1100, 9999)}@gmail.com";

            //Act
            var result = await _userService.GetByEmailAsync(email);

            //Assert
            Assert.IsNull(result);
        }

        #endregion

        private async Task<User?> CreateAndReturnUser()
        {
            var rand = new Random();
            var user = new User
            {
                Email = $"testuser{rand.Next(1000, 9999)}@gmail.com",
                Password = Password,
                IsActive = true,
                Role = "User",
                IsVerified = true
            };

            var userId = await _userRepo.AddAsync(user);
            return await _userRepo.GetByIdAsync(userId);
        }
        private User ReturnUser()
        {
            var rand = new Random();
            return new User
            {
                Email = $"testuser{rand.Next(1000, 9999)}@gmail.com",
                Password = Password,
                IsActive = true,
                Role = "User",
                IsVerified = true
            };
        }
    }

}
