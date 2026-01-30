namespace UserAuth.Api.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; }

    }
}
