using System.Security.Cryptography;
using System.Text;
using System.Transactions;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Repository;
using UserAuth.Api.Repository.EFCore;
using UserAuthWithOTP.DataLayer.Fixtures;

namespace UserAuthWithOTP.DataLayer.Repositories
{
    [DoNotParallelize]
    [TestClass]
    public class RefreshTokenRepositoryTests
    {
        private DatabaseFixture Fixture;
        private readonly IRefreshTokenRepository _tokenRepository = null!;
        private readonly IUserRepository _userRepository = null!;
        private TransactionScope _scope = null!;

        public RefreshTokenRepositoryTests()
        {
            Fixture = new DatabaseFixture();
            _userRepository = new UserRepository(Fixture.DbContext!);
            _tokenRepository = new RefreshTokenRepository(Fixture.DbContext!);


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
        public async Task AddAsync_WhenValid_ReturnInsertToken()
        {
            //Arrange 
            var token = await GenerateAndReturnToken();
            //Act 
            var tokenId = await _tokenRepository.AddAsync(token);
            //Assert
            Assert.IsGreaterThan(0, tokenId);
        }

        [TestMethod]
        public async Task GetByIdAsync_WhenTokenExists_ReturnToken()
        {
            //Arrange 
            var token = await GenerateAndReturnToken();
            var tokenId = await _tokenRepository.AddAsync(token);

            //Act 
            var tokenData = await _tokenRepository.GetByIdAsync(tokenId);

            //Assert
            Assert.IsNotNull(tokenData);
            Assert.AreEqual(token.TokenHash, tokenData.TokenHash);
        }

        [TestMethod]
        public async Task GetByTokenAsync_WhenTokenExists_ReturnToken()
        {
            //Arrange 
            var token = await GenerateAndReturnToken();
            await _tokenRepository.AddAsync(token);
            //Act 
            var tokenData = await _tokenRepository.GetByTokenAsync(token.TokenHash);

            //Assert
            Assert.IsNotNull(tokenData);
            Assert.AreEqual(token.IsRevoked, token.IsRevoked);
            Assert.AreEqual(token.ExpiredAt, token.ExpiredAt);
        }

        [TestMethod]
        public async Task GetAllByUserIdAsync_WhenTokenExistsWuthUser_ReturnToken()
        {
            //Arrange 
            var token = await GenerateAndReturnToken();
            await _tokenRepository.AddAsync(token);

            //Act 
            var tokenData = await _tokenRepository.GetAllByUserIdAsync(token.UserId);

            //Assert
            Assert.IsNotNull(tokenData);
            Assert.IsInstanceOfType<IEnumerable<RefreshToken>>(tokenData);
        }

        [TestMethod]
        public async Task UpdateAsync_WhenTokenExists_ReturnTrue()
        {
            //Arrange 
            var token = await GenerateAndReturnToken();
            var tokenId = await _tokenRepository.AddAsync(token);

            var update = new RefreshToken
            {
                Id = tokenId,
                TokenHash = token.TokenHash,
                UserId = token.UserId,
                IsRevoked = false,
                RevokedAt = token.RevokedAt,
                ExpiredAt = DateTime.UtcNow.AddDays(8)
            };

            //Act 
            var result = await _tokenRepository.UpdateAsync(tokenId, update);

            //Assert
            Assert.IsTrue(result);

            var tokenData = await _tokenRepository.GetByIdAsync(tokenId);
            Assert.IsNotNull(tokenData);
            Assert.AreEqual(update.ExpiredAt, tokenData.ExpiredAt);
        }

        [TestMethod]
        public async Task RevokeAsync_WhenTokenExists_ReturnTrue()
        {
            //Arrange 
            var token = await GenerateAndReturnToken();
            var tokenId = await _tokenRepository.AddAsync(token);
            //Act 
            var result = await _tokenRepository.RevokeAsync(tokenId);

            //Assert
            Assert.IsTrue(result);

            var tokenData = await _tokenRepository.GetByIdAsync(tokenId);
            Assert.IsNull(tokenData);
        }

        [TestMethod]
        public async Task DeleteAsync_WhenTokenExists_ReturnTrue()
        {
            //Arrange 
            var token = await GenerateAndReturnToken();
            var tokenId = await _tokenRepository.AddAsync(token);

            //Act 
            var result = await _tokenRepository.DeleteAsync(tokenId);

            //Assert
            Assert.IsTrue(result);

            var tokenData = await _userRepository.GetByIdAsync(tokenId);
            Assert.IsNull(tokenData);
        }

        #region Private Helper Method

        private string GenerateToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }

        public string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String((hash));
        }

        private async Task<RefreshToken> GenerateAndReturnToken()
        {
            var rand = new Random();
            var user = new User
            {
                Email = $"user{rand.Next(1000, 9999)}{rand.Next(1000, 9999)}@gmail.com",
                Password = "HelloWorld123!",
                Role = "user"
            };

            var userId = await _userRepository.AddAsync(user);

            return new RefreshToken
            {
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
                ExpiredAt = DateTime.UtcNow.AddDays(7),
                UserId = userId,
                TokenHash = HashToken(GenerateToken())
            };
        }

        #endregion


    }
}
