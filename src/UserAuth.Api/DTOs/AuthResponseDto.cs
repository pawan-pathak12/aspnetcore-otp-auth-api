namespace UserAuth.Api.DTOs
{
    public class AuthResponseDto
    {
        public string? HashedAccessToken { get; set; }
        public string? HashedRefreshToken { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}
