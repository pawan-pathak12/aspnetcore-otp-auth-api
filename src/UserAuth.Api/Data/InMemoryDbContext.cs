using UserAuth.Api.Entities;

namespace UserAuth.Api.Data
{
    public class InMemoryDbContext
    {
        public List<User> Users { get; set; } = new List<User>();
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public List<OtpVerification> OtpVerifications { get; set; } = new List<OtpVerification>();
    }
}
