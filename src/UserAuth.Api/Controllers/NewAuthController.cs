using Microsoft.AspNetCore.Mvc;
using UserAuth.Api.DTOs;
using UserAuth.Api.Helpers;
using UserAuth.Api.Interfaces.Service;

namespace UserAuth.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewAuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public NewAuthController(IAuthService authService)
        {
            this._authService = authService;
        }

        [HttpPost("register-sent-otp")]
        public async Task<IActionResult> RegisterAsync([FromBody] SentOtpDto request)
        {
            var result = await _authService.SendOtpAsync(request.Email);
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
            // add user 
            var response = await _authService.RegisterAsync(request.Email, request.Password);
            if (!response.IsSuccess)
            {
                return BadRequest(response.Error);
            }
            return Ok("User Added Successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto request)
        {
            var response = await _authService.LoginAsync(request.Email, request.Password);
            if (!response.IsSuccess)
            {
                return BadRequest(response.Error);
            }

            Response.Cookies.Append(
                "refreshToken",
                response.RefreshToken,
                CookieOptionsHelper.RefreshTokenCookie(response.ExpiryDate)
                );

            return Ok(new { accesstoken = response.AccessTokenhash });
        }
    }
}
