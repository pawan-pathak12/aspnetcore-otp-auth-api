using UserAuth.Api.Data;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Repository;

namespace UserAuth.Api.Repository.InMemory
{
    public class InMemoryUserRepo : IUserRepository
    {
        private readonly InMemoryDbContext _dbContext;
        private readonly List<User> _users;

        public InMemoryUserRepo(InMemoryDbContext dbContext)
        {
            this._dbContext = dbContext;
            _users = _dbContext.Users;
        }
        public Task<int> AddAsync(User user)
        {
            user.Id += 1;
            _users.Add(user);
            return Task.FromResult(user.Id);
        }

        public Task<bool> DeleteAsync(int id)
        {
            var user = _users.Find(x => x.Id == id);
            if (user == null)
            {
                return Task.FromResult(false);
            }
            _users.Remove(user);
            return Task.FromResult(true);
        }

        public Task<IEnumerable<User>> GetAllAsync()
        {
            var users = _users.ToList().AsEnumerable();
            if (users == null)
            {

            }
            return Task.FromResult(users);
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            var user = _users.Find(x => x.Email == email && x.IsActive);

            return Task.FromResult(user);
        }

        public Task<User?> GetByIdAsync(int id)
        {
            var user = _users.Find(x => x.Id == id && x.IsActive);
            if (user == null)
            {
                return null;
            }
            return Task.FromResult(user);
        }

        public Task<bool> UpdateAsync(User user)
        {
            if (user.Id <= 0)
            {
                return Task.FromResult(false);
            }
            var userData = _users.Find(x => x.Id == user.Id && x.IsActive);
            if (userData == null)
            {
                return Task.FromResult(false);
            }

            userData.Email = user.Email;
            userData.IsVerified = user.IsVerified;
            userData.Role = user.Role;
            userData.IsActive = user.IsActive;

            return Task.FromResult(true);
        }
    }
}
