using UserAuth.Api.Entities;

namespace UserAuth.Api.Interfaces.Repository
{
    public interface IUserRepository
    {
        Task<int> AddAsync(User user);
        Task<User?> GetByIdAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);

        Task<User?> GetByEmailAsync(string email);
    }

}
