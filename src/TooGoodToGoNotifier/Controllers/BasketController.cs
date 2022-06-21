using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TooGoodToGoNotifier.Dto;
using TooGoodToGoNotifier.Interfaces;

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
        public async Task<ActionResult<IEnumerable<BasketDto>>> GetFavoriteBasketsAsync([FromQuery] string userEmail)
        {
            IEnumerable<BasketDto> favoriteBaskets = await _basketService.GetFavoriteBasketsAsync(userEmail);
            return Ok(favoriteBaskets);
        }

        /// <summary>
        /// Set or remove a basket as favorite
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="id"></param>
        /// <param name="isFavorite"></param>
        /// <returns></returns>
        [HttpPatch("favorite/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Produces("application/json")]
        public async Task<ActionResult> SetBasketAsFavoriteAsync(string id, [FromQuery] string userEmail, [FromQuery] bool isFavorite)
        {
            await _basketService.SetBasketAsFavoriteAsync(userEmail, id, isFavorite);
            return Ok();
        }
    }
}
