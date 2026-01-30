using Microsoft.AspNetCore.Mvc;
using UserAuth.Api.DTOs;
using UserAuth.Api.Interfaces.Service;

namespace UserAuth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly ILogger<AuthController> _logger;
    private readonly IUserService _userService;

    public AuthController(IOtpService otpService, ILogger<AuthController> logger, IUserService userService)
    {
        _otpService = otpService;
        _logger = logger;
        this._userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {

        var user = await _userService.GetEmailAsync(request.Email);
        if (user != null)
        {
            _logger.LogWarning("User Already Exists");
            return BadRequest("User Already Exists");
        }

        await _otpService.GenerateAndSaveOtpAsync(request.Email);
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
}