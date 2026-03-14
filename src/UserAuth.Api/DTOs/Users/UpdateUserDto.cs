using System.ComponentModel.DataAnnotations;

namespace UserAuth.Api.DTOs.Users
{
    public class UpdateUserDto
    {
        public int Id { get; set; }
        [Required]
        public string? Password { get; set; }
        public bool IsActive { get; set; }
        public string? Role { get; set; }
    }
}
