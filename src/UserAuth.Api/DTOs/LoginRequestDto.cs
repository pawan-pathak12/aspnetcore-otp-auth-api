using System.ComponentModel.DataAnnotations;

namespace UserAuth.Api.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
