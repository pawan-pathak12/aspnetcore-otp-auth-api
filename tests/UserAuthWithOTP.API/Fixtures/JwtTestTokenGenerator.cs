using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserAuth.Api.Entities;

namespace UserAuthWithOTP.API.Fixtures
{
    public static class JwtTestTokenGenerator
    {
        public static string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("THIS_IS_A_SUPER_SECRET_KEY_CHANGE_LATER_BYOWNER")
                );

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                 new Claim(JwtRegisteredClaimNames.Sub , user.Id.ToString()),
                  new Claim(ClaimTypes.Role , user.Role),
                new Claim(JwtRegisteredClaimNames.Email, user.Email) ,
                new Claim(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: "JwtAuthLearning"
                , audience: "JwtAuthLearningUsers"
                , claims: claims
                , expires: DateTime.UtcNow.AddMinutes(15)
                , signingCredentials: cred
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
