using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UserAuth.Api.DTOs;
using UserAuth.Api.Entities;
using UserAuth.Api.Helpers;
using UserAuth.Api.Interfaces.Service;

namespace UserAuth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthService _authService;

    public AuthController(ILogger<AuthController> logger,
        IUserService userService, IAuthService authService)
    {
        _logger = logger;
        this._authService = authService;
    }

    [HttpPost("register-sent-otp")]
    public async Task<IActionResult> RegisterAsync([FromBody] SentOtpDto request)
    {
        var result = await _authService.SendOtpToRegisterAsync(request.Email);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }
        return Ok("Otp is sented , please check email ");
    }

    [HttpPost("register-verify-create-user")]
    public async Task<IActionResult> VerifyAndRegisterAsync([FromBody] VerifyOtpRequestDto request)
    {
        // first verify otp
        var result = await _authService.VerifyOtpAsync(request.Otp, request.Email);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }
        var user = new User
        {
            Email = request.Email,
            Password = request.Password
        };
        // add user 
        var response = await _authService.RegisterAsync(user);
        if (!response.IsSuccess)
        {
            return BadRequest(response.Error);
        }
        return Ok("User Added Successfully");
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto request)
    {
        var user = new User
        {
            Email = request.Email,
            Password = request.Password
        };
        var response = await _authService.LoginAsync(user);
        if (!response.IsSuccess)
        {
            return BadRequest(response.Error);
        }

        Response.Cookies.Append(
            "refreshToken",
            response.RefreshToken,
            CookieOptionsHelper.RefreshTokenCookie(response.ExpiryDate)
            );

        return Ok(new { accessToken = response.AccessTokenhash });
    }


    [HttpPost("refresh")]
    [EnableRateLimiting("RefreshTokenPolicy")]
    public async Task<IActionResult> Refresh()
    {
        //1.find the refresh token 
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized("Refresh token missing or invalid");
        }

        var response = await _authService.RotateRefreshTokenAsync(refreshToken);
        if (!response.IsSuccess)
        {
            return BadRequest($"Error : {response.Error}");
        }
        //return refresh token in cookie

        Response.Cookies.Append(
            "refreshToken",
            response.RefreshToken,
             CookieOptionsHelper.RefreshTokenCookie(response.ExpiryDate)
         );
        return Ok(new { accessToken = response.AccessTokenhash });
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

}