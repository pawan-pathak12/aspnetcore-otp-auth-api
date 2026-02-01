using UserAuth.Api.Entities;

namespace UserAuth.Api.Interfaces.Service
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);

    }
}
