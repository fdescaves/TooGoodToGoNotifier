using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TooGoodToGoNotifier.Interfaces;
using TooGoodToGoNotifier.Models;

namespace TooGoodToGoNotifier.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly ILogger<BasketController> _logger;
        private readonly IBasketService _basketService;

        public BasketController(ILogger<BasketController> logger, IBasketService basketService)
        {
            _logger = logger;
            _basketService = basketService;
        }

        /// <summary>
        /// Get favorite baskets
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpGet("favorite")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<Basket>>> GetFavoriteBasketsAsync([FromQuery] string userEmail)
        {
            IEnumerable<Basket> favoriteBaskets = await _basketService.GetFavoriteBasketsAsync(userEmail);
            return Ok(favoriteBaskets);
        }

        /// <summary>
        /// Set or remove baskets as favorite
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch("favorite")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Produces("application/json")]
        public async Task<IActionResult> UpdateBasketsFavoriteStatusAsync([FromQuery] string userEmail, [FromBody] UpdateBasketsFavoriteStatusRequest request)
        {
            await _basketService.UpdateBasketsFavoriteStatusAsync(userEmail, request.BasketsIds, request.SetAsFavorite);
            return Ok();
        }
    }
}
