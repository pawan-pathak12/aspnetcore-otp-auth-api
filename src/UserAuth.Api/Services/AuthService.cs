using Microsoft.AspNetCore.Identity;
using UserAuth.Api.DTOs;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Service;

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
        public async Task<(bool success, string errorMessage, AuthResponseDto? response)> RotateRefreshTokenAsync(string refreshToken)
        {
            var incomingTokenHash = _tokenService.HashToken(refreshToken);

            var existingToken = await _refreshTokenService.GetDataByTokenAsync(incomingTokenHash);

            // check if token exists or not 
            if (existingToken == null)
            {
                return (false, "Token not found ", null);
            }

            // if : reuse refreshToken detection 
            // then revoke all token for that user 
            if (existingToken.IsRevoked)
            {
                await _refreshTokenService.RevokeTokenAsync(existingToken.UserId);
                return (false, "Refresh token reuse detected , please relogin again.", null);
            }
            // check if expired or not
            if (existingToken.ExpiredAt < DateTime.UtcNow)
            {
                return (false, "Refresh token is expired or invalid", null);
            }

            //normal rotation of refreshtoken 
            var accesstoken = _tokenService.GenerateAccessToken(existingToken.User);
            existingToken.IsRevoked = true;

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

            return (true, "", new AuthResponseDto
            {
                HashedAccessToken = accesstoken,
                HashedRefreshToken = hashToken,
                ExpiredAt = token.ExpiredAt
            }
            );
        }

        public async Task<(bool success, string error, AuthResponseDto? response)> LoginWithJwtAsync(string email, string password)
        {
            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
            {
                return (false, "Invalid credentials", null);
            }

            // verfify  password 

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (result == PasswordVerificationResult.Failed)
            {
                return (false, "Invalid credentials", null);
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


            return (true, "", new AuthResponseDto
            {
                HashedAccessToken = _tokenService.HashToken(plainRefreshToken),
                HashedRefreshToken = refreshToken.TokenHash,
                ExpiredAt = refreshToken.ExpiredAt
            });

        }

        public async Task<(bool success, string errorMessage, int id)> RegisterWithJwt(string email, string password)
        {
            var user = await _userService.GetByEmailAsync(email);
            if (user != null)
            {
                return (false, "User already Exists", 0);
            }
            user = new User
            {
                Email = email
            };
            user.Password = _passwordHasher.HashPassword(user, password);

            var (success, id) = await _userService.CreateAsync(user);
            return (true, "", id);
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

        public async Task<(bool success, string ErrorMessage)> SendOtpAsync(string email)
        {
            var user = await _userService.GetByEmailAsync(email);
            if (user != null)
            {
                return (false, "User Already exists");
            }

            var isCreated = await _otpService.GenerateAndSaveOtpAsync(email);
            return (true, "");
        }

        public async Task<(bool success, string errorMessage)> VerifyOtpAsync(string otp, string email)
        {
            var isValid = await _otpService.VerifyOtpAndCreateUserAsync(otp, email);
            if (!isValid)
            {
                return (false, "Opt is invalid or expired");
            }
            return (true, "");
        }

        public async Task LoginAsync(int otp)
        {

        }

        #endregion

        #region Helper Method 


        #endregion
    }
}
