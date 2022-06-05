using System.Collections.Generic;
using System.Threading.Tasks;
using TooGoodToGoNotifier.Models;

namespace TooGoodToGoNotifier.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
    }
}
