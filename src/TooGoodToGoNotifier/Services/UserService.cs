using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TooGoodToGoNotifier.Entities;
using TooGoodToGoNotifier.Interfaces;

namespace TooGoodToGoNotifier.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly TooGoodToGoNotifierDbContext _dbContext;

        public UserService(ILogger<UserService> logger, TooGoodToGoNotifierDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _dbContext.Users.ToListAsync();
        }

        public async Task CreateUserAsync(string email)
        {
            var user = new User
            {
                Email = email,
                FavoriteBaskets = new List<string>()
            };

            _dbContext.Users.Add(user);

            await _dbContext.SaveChangesAsync();
        }
    }
}
