using System.ComponentModel.DataAnnotations;

namespace UserAuth.Api.DTOs
{
    public class VerifyOtpRequestDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Otp { get; set; }
        [Required]
        [MinLength(8, ErrorMessage = "Password length Should be more than or equal to 8")]
        public string? Password { get; set; }
    }
}
