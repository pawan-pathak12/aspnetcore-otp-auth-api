using System.ComponentModel.DataAnnotations;

namespace UserAuth.Api.DTOs
{

    public class SentOtpDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
