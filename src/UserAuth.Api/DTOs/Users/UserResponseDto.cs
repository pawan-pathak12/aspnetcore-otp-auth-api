namespace UserAuth.Api.DTOs.Users
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public DateTime CreateAt { get; set; }
        public bool IsActive { get; set; }
    }
}
