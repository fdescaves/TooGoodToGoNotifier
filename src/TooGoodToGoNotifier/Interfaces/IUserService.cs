using System.Collections.Generic;
using System.Threading.Tasks;
using TooGoodToGoNotifier.Entities;

namespace TooGoodToGoNotifier.Interfaces
{
    public interface IUserService
    {
        public Task<List<User>> GetAllUsersAsync();

        public Task CreateUserAsync(string email);
    }
}
