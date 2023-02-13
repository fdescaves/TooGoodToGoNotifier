using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TooGoodToGoNotifier.Entities;
using TooGoodToGoNotifier.Interfaces;
using TooGoodToGoNotifier.Models;

namespace TooGoodToGoNotifier.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsersAsync()
        {
            IEnumerable<User> users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Create a user
        /// </summary>
        /// <returns></returns>
        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Produces("application/json")]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserRequest request)
        {
            await _userService.CreateUserAsync(request.Email);
            return Ok();
        }
    }
}
