using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TooGoodToGoNotifier.Interfaces;
using TooGoodToGoNotifier.Models;

namespace TooGoodToGoNotifier.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly TooGoodToGoNotifierContext _dbContext;

        public UserService(ILogger<UserService> logger, TooGoodToGoNotifierContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public Task<List<User>> GetAllUsersAsync()
        {
            return _dbContext.Users.ToListAsync();
        }
    }
}
