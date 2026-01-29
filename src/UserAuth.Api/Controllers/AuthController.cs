using Microsoft.AspNetCore.Mvc;
using UserAuth.Api.DTOs;
using UserAuth.Api.Interfaces;

namespace UserAuth.Api.Controllers;

[ApiController]                    // ← ADD THIS!
[Route("api/[controller]")]        // ← ADD THIS!
public class AuthController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IOtpService otpService, ILogger<AuthController> logger)
    {
        _otpService = otpService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            await _otpService.GenerateAndSaveOtpAsync(request.Email);
            return Ok(new { message = "OTP sent to your email" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP");
            return StatusCode(500, new { message = "Failed to send OTP. Please try again." });
        }
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
    {
        var isValid = await _otpService.VerifyOtpAsync(request.Email, request.Otp);

        if (!isValid)
        {
            return BadRequest(new { message = "Invalid or expired OTP" });
        }

        return Ok(new { message = "OTP verified successfully" });
    }
}