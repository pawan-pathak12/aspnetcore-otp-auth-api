using Microsoft.AspNetCore.Identity;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Service;
using UserAuth.Api.Results;

namespace UserAuth.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IUserService _userService;
        private readonly IOtpService _otpService;
        private readonly PasswordHasher<User> _passwordHasher;


        public AuthService(ITokenService tokenService, IRefreshTokenService refreshTokenService,
            IUserService userService, IOtpService otpService)
        {
            this._tokenService = tokenService;
            this._refreshTokenService = refreshTokenService;
            this._userService = userService;
            this._otpService = otpService;
            _passwordHasher = new PasswordHasher<User>();
        }

        // instead of using tuple in return type : use : Result <T>

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
            var user = await _userService.GetByEmailAsync(email);
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

        public async Task<AuthResult> RegisterAsync(string email, string password)
        {
            var user = new User
            {
                Email = email,
                IsActive = true,
                CreateAt = DateTime.UtcNow,
                IsVerified = true
            };
            user.Password = _passwordHasher.HashPassword(user, password);

            var response = await _userService.CreateAsync(user);
            if (!response.IsSuccess)
            {
                return AuthResult.Failure("failed to create user");
            }
            return new AuthResult { IsSuccess = true };
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
        /*        use other service that is required for otp in here and
                 in controller this methods will be used to keep controller clean

        */

        public async Task<AuthResult> SendOtpAsync(string email)
        {
            var user = await _userService.GetByEmailAsync(email);
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

        public async Task LoginAsync(int otp)
        {

        }

        #endregion


    }
}
