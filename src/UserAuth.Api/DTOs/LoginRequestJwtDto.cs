using System.ComponentModel.DataAnnotations;

namespace UserAuth.Api.DTOs
{
    public class LoginRequestJwtDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
