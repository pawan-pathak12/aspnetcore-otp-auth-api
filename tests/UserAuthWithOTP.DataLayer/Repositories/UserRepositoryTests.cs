using System.Transactions;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Repository;
using UserAuth.Api.Repository.EFCore;
using UserAuthWithOTP.DataLayer.Fixtures;

namespace UserAuthWithOTP.DataLayer.Repositories
{
    [DoNotParallelize]
    [TestClass]
    public class UserRepositoryTests
    {
        private readonly IUserRepository _userRepository = null!;
        private TransactionScope _scope = null!;

        public UserRepositoryTests()
        {
            var fixture = new DatabaseFixture();
            _userRepository = new UserRepository(fixture.DbContext!);
        }

        [TestInitialize]
        public void Init()
        {
            _scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _scope.Dispose();
        }

        [TestMethod]
        public async Task AddAsync_WhenValid_InsertUserSuccessfully()
        {
            // Arrange
            var user = ReturnUser();

            // Act
            var result = await _userRepository.AddAsync(user);

            // Assert
            Assert.IsGreaterThan(0, result);
        }

        [TestMethod]
        public async Task GetByIdAsync_WhenUserExists_ReturnUser()
        {
            // Arrange
            var user = ReturnUser();
            var userId = await _userRepository.AddAsync(user);

            // Act
            var userData = await _userRepository.GetByIdAsync(userId);

            // Assert
            Assert.IsNotNull(userData);
            Assert.AreEqual(user.Email, userData.Email, "Email should match");
            Assert.AreEqual(user.Role, userData.Role, "Role should match");
        }

        [TestMethod]
        public async Task GetAllAsync_WhenUserExists_ReturnsUsers()
        {
            // Arrange
            var user1 = ReturnUser();
            var user2 = ReturnUser(); // different random email

            await _userRepository.AddAsync(user1);
            await _userRepository.AddAsync(user2);

            // Act
            var users = await _userRepository.GetAllAsync();

            // Assert
            Assert.IsNotNull(users);
            Assert.IsGreaterThan(0, users.Count());
            Assert.IsTrue(users.Any(u => u.Email == user1.Email));
            Assert.IsTrue(users.Any(u => u.Email == user2.Email));
        }

        [TestMethod]
        public async Task UpdateAsync_WhenUserExists_ReturnsTrue()
        {
            // Arrange
            var user = ReturnUser();
            var userId = await _userRepository.AddAsync(user);

            var updatedUser = new User
            {
                Id = userId,
                Email = user.Email,
                Password = "NewStrongPass123!",
                Role = "admin",
                IsActive = true,
                IsVerified = true,
                CreateAt = DateTime.UtcNow
            };

            // Act
            var result = await _userRepository.UpdateAsync(updatedUser);

            // Assert
            Assert.IsTrue(result, "Update should return true");

            // Verify change
            var refreshedUser = await _userRepository.GetByIdAsync(userId);
            Assert.AreEqual("admin", refreshedUser.Role);
            Assert.AreEqual("NewStrongPass123!", refreshedUser.Password);
        }

        [TestMethod]
        public async Task DeleteAsync_WhenUserExists_ReturnTrue()
        {
            // Arrange
            var user = ReturnUser();
            var userId = await _userRepository.AddAsync(user);

            // Act
            var result = await _userRepository.DeleteAsync(userId);

            // Assert
            Assert.IsTrue(result, "Delete should return true");

            // Verify user is gone
            var deletedUser = await _userRepository.GetByIdAsync(userId);
            Assert.IsNull(deletedUser, "User should no longer exist");
        }

        [TestMethod]
        public async Task GetByEmailAsync_WhenUserIsRegistered_ReturnUser()
        {
            // Arrange
            var user = ReturnUser();
            await _userRepository.AddAsync(user);

            // Act
            var foundUser = await _userRepository.GetByEmailAsync(user.Email);

            // Assert
            Assert.IsNotNull(foundUser, "Should find user by email");
            Assert.AreEqual(user.Email, foundUser.Email, "Emails should match");
            Assert.AreEqual(user.Role, foundUser.Role, "Role should match");
        }

        [TestMethod]
        public async Task GetByEmailAsync_WhenUserDoesNotExist_ReturnNull()
        {
            // Arrange
            string nonExistingEmail = "nonexistent@domain.com";

            // Act
            var foundUser = await _userRepository.GetByEmailAsync(nonExistingEmail);

            // Assert
            Assert.IsNull(foundUser, "Should return null for non-existing email");
        }

        private User ReturnUser()
        {
            var rand = new Random();
            return new User
            {
                Email = $"user{rand.Next(1000, 9999)}{rand.Next(1000, 9999)}@gmail.com",
                Password = "HelloWorld123!",
                Role = "Admin",
                IsActive = true
            };
        }
    }
}