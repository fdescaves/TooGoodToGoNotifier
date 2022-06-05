using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TooGoodToGo.Api.Interfaces;
using TooGoodToGo.Api.Models;
using TooGoodToGo.Api.Models.Responses;
using TooGoodToGoNotifier.Interfaces;
using TooGoodToGoNotifier.Jobs;
using TooGoodToGoNotifier.Models;
using Xunit;

namespace TooGoodToGoNotifier.Tests
{
    public class FavoriteBasketsWatcherJobTests
    {
        private readonly Mock<ILogger<FavoriteBasketsWatcherJob>> _loggerMock;
        private readonly Mock<ITooGoodToGoService> _tooGoodToGoServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Context _context;
        private readonly IMemoryCache _memoryCache;
        private readonly FavoriteBasketsWatcherJob _favoriteBasketsWatcherJob;

        public FavoriteBasketsWatcherJobTests()
        {
            _loggerMock = new Mock<ILogger<FavoriteBasketsWatcherJob>>();
            _tooGoodToGoServiceMock = new Mock<ITooGoodToGoService>();
            _emailServiceMock = new Mock<IEmailService>();
            _userServiceMock = new Mock<IUserService>();
            _userServiceMock.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(new List<User>
            {
                new User
                {
                    Email = "foo@bar.com",
                    FavoriteBaskets = new List<string> {"1001", "1003"}
                },
                new User
                {
                    Email = "foo+1@bar.com",
                    FavoriteBaskets = new List<string> {"1001", "1003"}
                },
                new User
                {
                    Email = "foo+2@bar.com",
                    FavoriteBaskets = new List<string> { "1002", "1003" }
                }
            });
            _context = new Context
            {
                AccessToken = "accessToken",
                TooGoodToGoUserId = 1
            };


            // Using a real instance of IMemoryCache is easier than mocking it
            var services = new ServiceCollection();
            services.AddMemoryCache();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            _memoryCache = serviceProvider.GetService<IMemoryCache>();

            _favoriteBasketsWatcherJob = new FavoriteBasketsWatcherJob(_loggerMock.Object, _tooGoodToGoServiceMock.Object, _emailServiceMock.Object,
                _userServiceMock.Object, _context, _memoryCache);
        }

        [Theory]
        [InlineData("1001")]
        [InlineData("1002")]
        public async Task WhenNoBasketsSeenAsAvailableShouldntSendEmailsToAnyRecipients(string notifiedBasketId)
        {
            MockGetBasketsResponse(notifiedBasketId, 0);

            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        }

        [Theory]
        [InlineData("1001", new string[] { "foo@bar.com", "foo+1@bar.com" })]
        [InlineData("1002", new string[] { "foo+2@bar.com" })]
        [InlineData("1003", new string[] { "foo@bar.com", "foo+1@bar.com", "foo+2@bar.com" })]
        public async Task WhenBasketSeenAsAvailableOnceShouldSendEmailToSubscribedRecipients(string notifiedBasketId, string[] expectedRecipients)
        {
            MockGetBasketsResponse(notifiedBasketId, 1);

            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string[]>(x => Enumerable.SequenceEqual(x, expectedRecipients))), Times.Once);
        }

        [Fact]
        public async Task WhenBasketSeenAsAvailableOnceAndNoUsersSubscribedShouldntSendAnyEmail()
        {
            MockGetBasketsResponse("1000", 1);

            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        }

        [Theory]
        [InlineData("1001", new string[] { "foo@bar.com", "foo+1@bar.com" })]
        [InlineData("1002", new string[] { "foo+2@bar.com" })]
        public async Task WhenBasketSeenAsAvailableTwiceShouldOnlySendEmailOnceToSubscribedRecipients(string notifiedBasketId, string[] expectedRecipients)
        {
            MockGetBasketsResponse(notifiedBasketId, 1);

            await _favoriteBasketsWatcherJob.Invoke();
            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string[]>(x => Enumerable.SequenceEqual(x, expectedRecipients))), Times.Once);
        }

        [Theory]
        [InlineData("1001", new string[] { "foo@bar.com", "foo+1@bar.com" })]
        [InlineData("1002", new string[] { "foo+2@bar.com" })]
        public async Task WhenBasketSeenAsAvailableThenOutOfStockThenAvailableShouldSendEmailTwiceToSubscribedRecipients(string notifiedBasketId, string[] expectedRecipients)
        {
            MockGetBasketsResponse(notifiedBasketId, 1);

            await _favoriteBasketsWatcherJob.Invoke();

            MockGetBasketsResponse(notifiedBasketId, 0);

            await _favoriteBasketsWatcherJob.Invoke();

            MockGetBasketsResponse(notifiedBasketId, 1);

            await _favoriteBasketsWatcherJob.Invoke();

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string[]>(x => Enumerable.SequenceEqual(x, expectedRecipients))), Times.Exactly(2));
        }

        private void MockGetBasketsResponse(string notifiedBasketId, int availableItems)
        {
            var getBasketsResponse = new GetBasketsResponse
            {
                Items = new List<TgtgBasket>
                {
                    new TgtgBasket
                    {
                        ItemsAvailable = availableItems,
                        Item = new TgtgItem
                        {
                            ItemId = notifiedBasketId
                        }
                    }
                }
            };

            _tooGoodToGoServiceMock.Setup(x => x.GetFavoriteBasketsAsync(_context.AccessToken, _context.TooGoodToGoUserId)).ReturnsAsync(getBasketsResponse);
        }
    }
}
