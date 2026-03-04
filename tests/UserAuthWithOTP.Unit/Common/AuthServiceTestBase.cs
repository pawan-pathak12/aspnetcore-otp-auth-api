using Microsoft.AspNetCore.Identity;
using UserAuth.Api.Data;
using UserAuth.Api.Entities;
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
        protected InMemoryUserRepo _userRepo = null!;
        protected IOtpService _otpService = null!;
        protected PasswordHasher<User> _passwordHasher = null!;
        protected IAuthService _authService = null!;


        // remove unnecessary use of interface in any service and repo : like in inmeory repo : ItokeSerice : crerate service for it ... make proper planning for it 

        [TestInitialize]
        public void TestInit()
        {
            var dbContext = new InMemoryDbContext();

            _userRepo = new InMemoryUserRepo(dbContext);
            //    _refreshTokenService = new RefreshTokenService(, dbContext. );

            _authService = new AuthService(_tokenService, _refreshTokenService, _otpService, _userRepo);
        }
    }
}
