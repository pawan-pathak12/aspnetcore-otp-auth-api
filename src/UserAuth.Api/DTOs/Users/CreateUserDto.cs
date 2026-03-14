using System.ComponentModel.DataAnnotations;

namespace UserAuth.Api.DTOs.Users
{
    public class CreateUserDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }

    }
}
