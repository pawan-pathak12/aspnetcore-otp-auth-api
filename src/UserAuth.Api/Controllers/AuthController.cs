using Microsoft.AspNetCore.Mvc;
using UserAuth.Api.DTOs;
using UserAuth.Api.Helpers;
using UserAuth.Api.Interfaces.Service;

namespace UserAuth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IUserService _userService;
    private readonly IAuthService _authService;

    public AuthController(ILogger<AuthController> logger,
        IUserService userService, IAuthService authService)
    {
        _logger = logger;
        this._userService = userService;
        this._authService = authService;
    }

    #region Otp based 


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {

        var (success, errorMessage) = await _authService.SendOtpAsync(request.Email);

        if (!success)
        {
            return BadRequest(errorMessage);
        }
        return Ok(new { message = "OTP sent to your email" });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
    {
        var (success, errorMessage) = await _authService.VerifyOtpAsync(request.Otp, request.Email);
        if (!success)
        {
            return BadRequest(errorMessage);
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
        var (success, errorMessage, response) = await _authService.LoginWithJwtAsync(request.Email, request.Password);
        if (!success)
        {
            return BadRequest(errorMessage);
        }

        return Ok(new
        {
            Token = response?.HashedAccessToken,
            refreshToken = response?.HashedRefreshToken
        });
    }

    //REGISTER 
    [HttpPost("register-jwt")]
    public async Task<IActionResult> RegisterJWt([FromBody] RegisterRequestDto request)
    {
        var (success, errorMessage, id) = await _authService.RegisterWithJwt(request.Email, request.Password);

        if (!success)
        {
            return BadRequest(errorMessage);
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
        var storedTokenData = await _authService.LogoutSessionAsync(refreshToken);

        // delete cookie 
        Response.Cookies.Delete("refreshToken");

        return Ok("Log out successfully");
    }

    #endregion
}