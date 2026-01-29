using System.ComponentModel.DataAnnotations;

namespace UserAuth.Api.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        public string? Email { get; set; }
    }
}
