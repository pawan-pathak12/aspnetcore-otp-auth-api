using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserAuth.Api.Data;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Repository;
using UserAuth.Api.Interfaces.Service;

namespace UserAuth.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _appDbContext;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(IUserRepository userRepository, AppDbContext appDbContext)
        {
            this._userRepository = userRepository;
            this._appDbContext = appDbContext;
            _passwordHasher = new PasswordHasher<User>();
        }
        public async Task<(bool success, int id)> CreateAsync(User user)
        {
            var result = await _userRepository.AddAsync(user);
            if (result <= 0)
            {
                return (false, 0);
            }
            return (true, result);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _userRepository.DeleteAsync(id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
        }
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(User user)
        {
            return await _userRepository.UpdateAsync(user);

        }

        public async Task<bool> LoginAsync(string email)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
            {
                return false;
            }
            if (!user.IsVerified)
            {
                return false;
            }
            return true;
        }

        public async Task<string?> HashPassword(string email, string password)
        {
            var user = await GetByEmailAsync(email);
            if (user == null)
            {
                return null;
            }
            var hash = _passwordHasher.HashPassword(user, password);
            return hash;
        }

        public async Task<string?> VerifyHashPasswordAsync(string email, string password)
        {

        }


    }
}
