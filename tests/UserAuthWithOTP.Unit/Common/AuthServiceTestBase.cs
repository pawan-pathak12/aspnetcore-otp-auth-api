using Microsoft.Extensions.Configuration;
using Moq;
using UserAuth.Api.Data;
using UserAuth.Api.Interfaces.Service;
using UserAuth.Api.Repository.InMemory;
using UserAuth.Api.Services;

namespace UserAuthWithOTP.Unit.Common
{
    public class AuthServiceTestBase
    {
        // authservice : tokenservice , irefoken repo , optpserv     , iuserrepo , passwordhasher ,

        protected ITokenService _tokenService = null!;
        protected IRefreshTokenService _refreshTokenService = null!;
        protected IOtpService _otpService = null!;
        protected InMemoryUserRepo _userRepo = null!;
        protected InMemoryRefreshTokenRepo _inMemoryRefreshToken = null!;
        protected IConfiguration configuration = null!;

        [TestInitialize]
        public void TestInit()
        {
            var dbContext = new InMemoryDbContext();
            var configurationMock = new Mock<IConfiguration>(configuration);

            _tokenService = new TokenService(configurationMock.Object);

            _inMemoryRefreshToken = new RefreshTokenRepository(dbContext);
            _refreshTokenService = new RefreshTokenService(_inMemoryRefreshToken);

        }
    }
}
