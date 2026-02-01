namespace UserAuth.Api.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string TokenHash { get; set; } = string.Empty;

        public int UserId { get; set; }
        public User? User { get; set; }


        public bool IsRevoked { get; set; } = false;
        public DateTime ExpiredAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;




    }
}
