using System.Text.Json.Serialization;

namespace UserAuth.Api.DTOs
{
    public class AuthResponseDto
    {
        [JsonPropertyName("accessToken")]
        public string? AccessToken { get; set; }
        public string? HashedRefreshToken { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}
