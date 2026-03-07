using Microsoft.AspNetCore.Identity;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Repository;
using UserAuth.Api.Interfaces.Service;
using UserAuth.Api.Results;

namespace UserAuth.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(IUserRepository userRepository)
        {
            this._userRepository = userRepository;
            _passwordHasher = new PasswordHasher<User>();
        }
        public async Task<Result> CreateAsync(User user)
        {
            var id = await _userRepository.AddAsync(user);
            if (id <= 0)
            {
                return Result.Failure("Uner not created");
            }
            return Result.Success(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _userRepository.DeleteAsync(id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
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
            var user = await _userRepository.GetByEmailAsync(email);
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




    }
}
