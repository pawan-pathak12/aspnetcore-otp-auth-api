using Microsoft.AspNetCore.Identity;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Repository;
using UserAuth.Api.Interfaces.Service;
using UserAuth.Api.Results;

namespace UserAuth.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IOtpService _otpService;
        private readonly IUserRepository _userRepo;
        private readonly PasswordHasher<User> _passwordHasher;


        public AuthService(ITokenService tokenService, IRefreshTokenService refreshTokenService
             , IOtpService otpService, IUserRepository userRepo)
        {
            this._tokenService = tokenService;
            this._refreshTokenService = refreshTokenService;
            this._otpService = otpService;
            this._userRepo = userRepo;
            _passwordHasher = new PasswordHasher<User>();
        }


        #region JWT 
        /*        use other service that is required for otp in here and
                in controller this methods will be used to keep controller clean
       */
        public async Task<AuthResult> RotateRefreshTokenAsync(string refreshToken)
        {
            var incomingTokenHash = _tokenService.HashToken(refreshToken);

            var existingToken = await _refreshTokenService.GetDataByTokenAsync(incomingTokenHash);

            // check if token exists or not 
            if (existingToken == null)
            {
                return AuthResult.Failure($"Token not found or invalid");
            }

            // if : reuse refreshToken detection 
            // then revoke all token for that user 
            if (existingToken.IsRevoked)
            {
                await _refreshTokenService.RevokeTokenAsync(existingToken.UserId);
                return AuthResult.Failure("Refresh token reuse detected , please relogin again.");
            }
            // check if expired or not
            if (existingToken.ExpiredAt < DateTime.UtcNow)
            {
                return AuthResult.Failure("Refresh token is expired or invalid");
            }

            //normal rotation of refreshtoken 
            existingToken.IsRevoked = true;

            var accesstoken = _tokenService.GenerateAccessToken(existingToken.User);
            var plainrefreshToken = _tokenService.GenerateRefreshTokenAsync();
            var hashToken = _tokenService.HashToken(plainrefreshToken);

            var token = new RefreshToken
            {
                UserId = existingToken.UserId,
                TokenHash = hashToken,
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
                ExpiredAt = DateTime.UtcNow.AddDays(7)
            };

            await _refreshTokenService.CreateTokenAsync(token);

            return AuthResult.Success(
                _tokenService.HashToken(accesstoken),
                plainrefreshToken, token.ExpiredAt
                );

        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null)
            {
                return AuthResult.Failure("Invalid credentials");
            }

            // verfify  password 

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (result == PasswordVerificationResult.Failed)
            {
                return AuthResult.Failure("Invalid credentials");
            }
            var token = _tokenService.GenerateAccessToken(user);

            // generate refresh token 
            var plainRefreshToken = _tokenService.GenerateRefreshTokenAsync();

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = _tokenService.HashToken(plainRefreshToken),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
                ExpiredAt = DateTime.UtcNow.AddDays(7)
            };

            await _refreshTokenService.CreateTokenAsync(refreshToken);


            return AuthResult.Success(
                _tokenService.HashToken(token),
                refreshToken.TokenHash,
                refreshToken.ExpiredAt);
        }

        public async Task<string> LogoutSessionAsync(string refreshToken)
        {
            var storedTokenData = await _refreshTokenService.GetDataByTokenAsync(refreshToken);
            if (storedTokenData == null)
            {
                return "Token is already invalid";
            }
            storedTokenData.IsRevoked = true;

            await _refreshTokenService.UpdateAsync(storedTokenData.Id, storedTokenData);
            return $"token is Revoked";
        }


        #endregion

        #region OTP 

        public async Task<AuthResult> SendOtpToRegisterAsync(string email)
        {
            var user = await _userRepo.GetByEmailAsync(email);
            if (user != null)
            {
                return AuthResult.Failure("User already Exists");
            }

            var isCreated = await _otpService.GenerateAndSaveOtpAsync(email);
            return new AuthResult { IsSuccess = true };
        }

        public async Task<AuthResult> VerifyOtpAsync(string otp, string email)
        {
            var isValid = await _otpService.VerifyOtpAsync(email, otp);
            if (!isValid)
            {
                return AuthResult.Failure("Opt is invalid or expired");
            }
            return new AuthResult { IsSuccess = true };
        }

        public async Task<AuthResult> RegisterAsync(User user)
        {
            var existingUser = await _userRepo.GetByEmailAsync(user?.Email);
            if (existingUser != null)
            {
                return AuthResult.Failure("User aleady exists.");
            }
            existingUser.Email = user.Email;
            existingUser.Role = "User";

            var passwordHssh = _passwordHasher.HashPassword(user, user.Password);
            existingUser.Password = passwordHssh;
            await _userRepo.AddAsync(existingUser);

            return AuthResult.Success("user register Successfully");

        }

        #endregion


    }
}
