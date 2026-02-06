using UserAuth.Api.Entities;

namespace UserAuth.Api.Interfaces.Service
{
    public interface IUserService
    {

        Task<(bool success, int id)> CreateAsync(User user);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<bool> LoginAsync(string email);
        Task<string?> HashPassword(string email, string password);
    }
}
