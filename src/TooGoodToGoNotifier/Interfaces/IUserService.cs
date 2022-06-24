using System.Collections.Generic;
using System.Threading.Tasks;
using TooGoodToGoNotifier.Entities;

namespace TooGoodToGoNotifier.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
    }
}
