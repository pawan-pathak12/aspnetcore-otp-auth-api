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

        public async Task<AuthResult> RotateRefreshTokenAsync(string refreshToken)
        {
            var incomingTokenHash = _tokenService.HashToken(refreshToken);

            var existingToken = await _refreshTokenService.GetDataByTokenAsync(incomingTokenHash);

            // check if token exists or not 
            if (existingToken == null)
            {
                return AuthResult.Failure($"Token not found or invalid");
            }
            if (existingToken.User == null)
            {
                return AuthResult.Failure("User is null..");
            }

            // if : reuse refreshToken detection :then revoke all token for that user 

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

            var user = existingToken.User;

            var accesstoken = _tokenService.GenerateAccessToken(user);
            var plainrefreshToken = _tokenService.GenerateRefreshTokenAsync();
            var hashRefToken = _tokenService.HashToken(plainrefreshToken);

            var token = new RefreshToken
            {
                UserId = existingToken.UserId,
                TokenHash = hashRefToken,
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
                ExpiredAt = DateTime.UtcNow.AddDays(7),
                User = user
            };

            await _refreshTokenService.CreateTokenAsync(token);

            return AuthResult.Success(
                accesstoken,
                plainrefreshToken, token.ExpiredAt
                );

        }

        public async Task<AuthResult> LoginAsync(User user)
        {
            var storedUser = await _userRepo.GetByEmailAsync(user?.Email);
            if (storedUser == null)
            {
                return AuthResult.Failure("Invalid credentials");
            }

            // verfify  password 

            var result = _passwordHasher.VerifyHashedPassword(storedUser, storedUser.Password, user?.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return AuthResult.Failure("Invalid credentials");
            }
            else if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                storedUser.Password = _passwordHasher.HashPassword(storedUser, user.Password);
                await _userRepo.UpdateAsync(storedUser);
            }


            var token = _tokenService.GenerateAccessToken(storedUser);

            // generate refresh token 
            var plainRefreshToken = _tokenService.GenerateRefreshTokenAsync();

            var refreshToken = new RefreshToken
            {
                UserId = storedUser.Id,
                TokenHash = _tokenService.HashToken(plainRefreshToken),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
                ExpiredAt = DateTime.UtcNow.AddDays(7)
            };

            await _refreshTokenService.CreateTokenAsync(refreshToken);


            return AuthResult.Success(
                token,
                plainRefreshToken,
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

            var isCreated = await _otpService.GenerateAndSendOtpAsync(email);
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
            var newUser = new User
            {
                Email = user.Email,
                Password = _passwordHasher.HashPassword(user, user.Password),
                Role = "User",
                IsActive = true,
                IsVerified = true,
                CreateAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(newUser);

            return AuthResult.Success("user register Successfully");

        }

        #endregion


    }
}
