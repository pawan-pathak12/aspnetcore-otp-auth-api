namespace UserAuth.Api.Entities
{
    public class OtpVerification
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? OtpCode { get; set; }
        public DateTime ExpiryTime { get; set; }
        public string? IpAddress { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
