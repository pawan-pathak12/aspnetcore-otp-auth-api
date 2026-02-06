using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserAuth.Api.DTOs;
using UserAuth.Api.Entities;
using UserAuth.Api.Helpers;
using UserAuth.Api.Interfaces.Service;

namespace UserAuth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IOtpService _otpService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<AuthController> _logger;
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly IAuthService _authService;

    public AuthController(IOtpService otpService, IRefreshTokenService refreshTokenService, ILogger<AuthController> logger,
        IUserService userService, ITokenService tokenService, IAuthService authService)
    {
        this._passwordHasher = new PasswordHasher<User>();
        _otpService = otpService;
        this._refreshTokenService = refreshTokenService;
        _logger = logger;
        this._userService = userService;
        this._tokenService = tokenService;
        this._authService = authService;
    }

    #region Otp based 


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {

        var user = await _userService.GetByEmailAsync(request.Email);
        if (user != null)
        {
            _logger.LogWarning("User Already Exists");
            return BadRequest("User Already Exists");
        }

        var isOtpCreated = await _otpService.GenerateAndSaveOtpAsync(request.Email);
        if (!isOtpCreated)
        {
            return BadRequest("Too many OTP requests. Try again later.");
        }
        return Ok(new { message = "OTP sent to your email" });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
    {

        var isValid = await _otpService.VerifyOtpAndCreateUserAsync(request.Email, request.Otp);

        if (!isValid)
        {
            return BadRequest(new { message = "Invalid or expired OTP" });
        }


        return Ok(new { message = "Email verified and user registered successfully" });
    }

    [HttpPost("login-otp")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var sucess = await _userService.LoginAsync(request.Email);
        if (!sucess)
        {
            return Unauthorized("Invalid login or email not verified");
        }
        return Ok(new { message = "Login Sucessful" });
    }

    #endregion

    #region Jwt 
    //login 
    [HttpPost("LoginJWt")]
    public async Task<IActionResult> LoginJwt([FromBody] LoginRequestJwtDto request)
    {
        var user = await _userService.GetByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized("Invalid credentials");
        }

        //hash password 
        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid credentials");
        }

        var token = _tokenService.GenerateAccessToken(user);

        //generate refresh token
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.GenerateRefreshTokenAsync(),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            ExpiredAt = DateTime.UtcNow.AddDays(1)
        };

        await _refreshTokenService.CreateTokenAsync(refreshToken);
        return Ok(new { Token = token, refreshToken = refreshToken.TokenHash });
    }

    //REGISTER 
    [HttpPost("register-jwt")]
    public async Task<IActionResult> RegisterJWt(string email, string password)
    {
        var existingUser = await _userService.GetByEmailAsync(email);
        if (existingUser != null)
        {
            return BadRequest("User Already exists");
        }
        var user = new User
        {
            Email = email
        };
        var passwordHash = _passwordHasher.HashPassword(user, password);
        user.Password = passwordHash;
        var (success, id) = await _userService.CreateAsync(user);
        if (!success)
        {
            return BadRequest("Failed to create user");
        }
        return Ok("User Created");

    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        //1.find the refresh token 
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized("Refresh token missing or invalid");
        }

        var (sucess, errorMessage, result) = await _authService.RotateRefreshTokenAsync(refreshToken);
        if (!sucess)
        {
            return BadRequest($"Error : {errorMessage}");
        }
        //return refresh token in cookie

        Response.Cookies.Append(
            "refreshToken",
            result.HashedRefreshToken,
             CookieOptionsHelper.RefreshTokenCookie(result.ExpiredAt)
         );
        return Ok(new { accessToken = result.HashedAccessToken });
    }
    [HttpPost("Logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest("Token not found or invalid");
        }

        var storedTokenData = await _refreshTokenService.GetDataByTokenAsync(refreshToken);
        if (storedTokenData == null)
        {
            return Ok("Token is already Invalid");
        }
        storedTokenData.IsRevoked = true;

        // add update method 

        // delete cookie 
        Response.Cookies.Delete("refreshToken");

        return Ok("Log out successfully");
    }

    #endregion
}