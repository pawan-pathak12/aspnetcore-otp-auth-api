using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserAuth.Api.DTOs;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Service;

namespace UserAuth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IOtpService _otpService;
    private readonly ILogger<AuthController> _logger;
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public AuthController(IOtpService otpService, ILogger<AuthController> logger, IUserService userService, ITokenService tokenService)
    {
        this._passwordHasher = new PasswordHasher<User>();
        _otpService = otpService;
        _logger = logger;
        this._userService = userService;
        this._tokenService = tokenService;
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

    [HttpPost("login")]
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
        return Ok(new { Token = token });
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

    #endregion
}