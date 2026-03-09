using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Repository;

namespace UserAuthWithOTP.API
{
    public class TestDataBuilder
    {
        private readonly IUserRepository _userRepository;

        public const string DefaultPassword = "Apple@@211";

        public TestDataBuilder(IServiceProvider serviceProvider)
        {
            _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task<User> CreateAndReturnUser()
        {
            var user = new User();
            var rand = new Random();

            user.Password = HashPassword(DefaultPassword);
            user.Email = $"user+{rand.Next(10000, 99999)}@gmail.com";
            user.Role = "Admin";

            var userId = await _userRepository.AddAsync(user);
            var userData = await _userRepository.GetByEmailAsync(user.Email);

            return new User
            {
                Email = userData.Email,
                Role = userData.Role,
                Id = userId
            };
        }

    }
}
