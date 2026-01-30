using UserAuth.Api.Entities;

namespace UserAuth.Api.Interfaces.Service
{
    public interface IUserService
    {

        Task<(bool success, int id)> CreateAsync(User user);
        Task<User?> GetEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
    }
}
