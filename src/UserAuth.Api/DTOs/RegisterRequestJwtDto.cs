using System.ComponentModel.DataAnnotations;

namespace UserAuth.Api.DTOs
{
    public class RegisterRequestJwtDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
