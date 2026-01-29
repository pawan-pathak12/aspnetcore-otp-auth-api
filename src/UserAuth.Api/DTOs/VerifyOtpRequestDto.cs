using System.ComponentModel.DataAnnotations;

namespace UserAuth.Api.DTOs
{
    public class VerifyOtpRequestDto
    {
        [Required]
        public string? Email { get; set; }
        public string? Otp { get; set; }
    }
}
