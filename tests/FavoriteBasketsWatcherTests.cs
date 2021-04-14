using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TooGoodToGoNotifier.Api;
using TooGoodToGoNotifier.Api.Responses;

namespace TooGoodToGoNotifier.Tests
{
    [TestFixture]
    public class FavoriteBasketsWatcherTests
    {
        private Mock<ILogger<FavoriteBasketsWatcher>> _loggerMock;
        private Mock<ITimer> _timerMock;
        private Mock<ITooGoodToGoApiService> _tooGoodToGoApiServiceMock;
        private Mock<IEmailNotifier> _emailNotifierMock;
        private AuthenticationContext _authenticationContext;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<FavoriteBasketsWatcher>>();
            _timerMock = new Mock<ITimer>();
            _tooGoodToGoApiServiceMock = new Mock<ITooGoodToGoApiService>();
            _emailNotifierMock = new Mock<IEmailNotifier>();
            _authenticationContext = new AuthenticationContext
            {
                AccessToken = "foo",
                RefreshToken = "bar",
                UserId = 1
            };
        }

        [Test]
        public void Start_Should_StartTimer()
        {
            var favoriteBasketsWatcher = new FavoriteBasketsWatcher(_loggerMock.Object, _timerMock.Object, _tooGoodToGoApiServiceMock.Object, _emailNotifierMock.Object);
            favoriteBasketsWatcher.Start();

            _timerMock.Verify(x => x.Start(), Times.Once);
        }

        [Test]
        public void FavoriteBasketsWatcher_When_TimeIsElapsed_Should_CallGetFavoriteBaskets_And_StartTimer()
        {
            var getBasketsResponse = new GetBasketsResponse
            {
                Items = new List<Basket>()
            };

            _tooGoodToGoApiServiceMock.Setup(x => x.Authenticate()).Returns(_authenticationContext);
            _tooGoodToGoApiServiceMock.Setup(x => x.GetFavoriteBaskets(It.IsAny<string>(), It.IsAny<int>())).Returns(getBasketsResponse);

            var favoriteBasketsWatcher = new FavoriteBasketsWatcher(_loggerMock.Object, _timerMock.Object, _tooGoodToGoApiServiceMock.Object, _emailNotifierMock.Object);
            favoriteBasketsWatcher.Start();

            _timerMock.Raise(x => x.Elapsed += null, (EventArgs)null);

            _tooGoodToGoApiServiceMock.Verify(x => x.GetFavoriteBaskets(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            _timerMock.Verify(x => x.Start(), Times.Exactly(2));

            _timerMock.Raise(x => x.Elapsed += null, (EventArgs)null);

            _tooGoodToGoApiServiceMock.Verify(x => x.GetFavoriteBaskets(It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(2));
            _timerMock.Verify(x => x.Start(), Times.Exactly(3));
        }

        [Test]
        public void FavoriteBasketsWatcher_Should_CallGetFavoriteBasketsWithAuthenticationContextData()
        {
            var getBasketsResponse = new GetBasketsResponse
            {
                Items = new List<Basket>()
            };

            _tooGoodToGoApiServiceMock.Setup(x => x.Authenticate()).Returns(_authenticationContext);
            _tooGoodToGoApiServiceMock.Setup(x => x.GetFavoriteBaskets(It.IsAny<string>(), It.IsAny<int>())).Returns(getBasketsResponse);

            var favoriteBasketsWatcher = new FavoriteBasketsWatcher(_loggerMock.Object, _timerMock.Object, _tooGoodToGoApiServiceMock.Object, _emailNotifierMock.Object);
            favoriteBasketsWatcher.Start();

            _timerMock.Raise(x => x.Elapsed += null, (EventArgs)null);

            _tooGoodToGoApiServiceMock.Verify(x => x.GetFavoriteBaskets(_authenticationContext.AccessToken, _authenticationContext.UserId), Times.Once);
        }

        [Test]
        public void FavoriteBasketsWatcher_When_OneBasketIsAvailableAndAnotherIsNot_Should_OnlyNotifyAvailableBasket()
        {
            var getBasketsResponse = new GetBasketsResponse
            {
                Items = new List<Basket>
                {
                    new Basket
                    {
                        DisplayName = "Basket N°1",
                        AvailableItems = 1,
                        Item = new Item
                        {
                            ItemId = 1
                        }
                    },
                    new Basket
                    {
                        DisplayName = "Basket N°2",
                        AvailableItems = 0,
                        Item = new Item
                        {
                            ItemId = 2
                        }
                    }
                }
            };

            _tooGoodToGoApiServiceMock.Setup(x => x.Authenticate()).Returns(_authenticationContext);
            _tooGoodToGoApiServiceMock.Setup(x => x.GetFavoriteBaskets(It.IsAny<string>(), It.IsAny<int>())).Returns(getBasketsResponse);

            var favoriteBasketsWatcher = new FavoriteBasketsWatcher(_loggerMock.Object, _timerMock.Object, _tooGoodToGoApiServiceMock.Object, _emailNotifierMock.Object);
            favoriteBasketsWatcher.Start();

            _timerMock.Raise(x => x.Elapsed += null, (EventArgs)null);

            // Only one basket should have been notified, the basket N°1 that has 1 available item.
            _emailNotifierMock.Verify(x => x.Notify(It.Is<List<Basket>>(x => x.Count == 1 && x[0].Item.ItemId == 1)), Times.Once);
        }

        [Test]
        public void FavoriteBasketsWatcher_When_BasketIsSeenTwoTimesAsAvailable_Should_NotifyOnce()
        {
            var getBasketsResponse = new GetBasketsResponse
            {
                Items = new List<Basket>
                {
                    new Basket
                    {
                        DisplayName = "Basket N°1",
                        AvailableItems = 1,
                        Item = new Item
                        {
                            ItemId = 1
                        }
                    }
                }
            };

            _tooGoodToGoApiServiceMock.Setup(x => x.Authenticate()).Returns(_authenticationContext);
            _tooGoodToGoApiServiceMock.Setup(x => x.GetFavoriteBaskets(It.IsAny<string>(), It.IsAny<int>())).Returns(getBasketsResponse);

            var favoriteBasketsWatcher = new FavoriteBasketsWatcher(_loggerMock.Object, _timerMock.Object, _tooGoodToGoApiServiceMock.Object, _emailNotifierMock.Object);
            favoriteBasketsWatcher.Start();

            _timerMock.Raise(x => x.Elapsed += null, (EventArgs)null);
            _timerMock.Raise(x => x.Elapsed += null, (EventArgs)null);

            // Notify should have been called only once.
            _emailNotifierMock.Verify(x => x.Notify(It.Is<List<Basket>>(x => x.Count == 1 && x[0].Item.ItemId == 1)), Times.Once);
        }

        [Test]
        public void FavoriteBasketsWatcher_When_BasketIsSeenAsAvailable_Then_SeenAsNotAvailable_Then_SeenAsAvailable_Should_NotifyTwice()
        {
            _tooGoodToGoApiServiceMock.Setup(x => x.Authenticate()).Returns(_authenticationContext);
            _tooGoodToGoApiServiceMock.Setup(x => x.GetFavoriteBaskets(It.IsAny<string>(), It.IsAny<int>())).Returns(GetBasketsResponse(1));

            var favoriteBasketsWatcher = new FavoriteBasketsWatcher(_loggerMock.Object, _timerMock.Object, _tooGoodToGoApiServiceMock.Object, _emailNotifierMock.Object);
            favoriteBasketsWatcher.Start();

            _timerMock.Raise(x => x.Elapsed += null, (EventArgs)null);

            // Notify should be called once.
            _emailNotifierMock.Verify(x => x.Notify(It.Is<List<Basket>>(x => x.Count == 1 && x[0].Item.ItemId == 1)), Times.Once);

            _tooGoodToGoApiServiceMock.Setup(x => x.GetFavoriteBaskets(It.IsAny<string>(), It.IsAny<int>())).Returns(GetBasketsResponse(0));
            _timerMock.Raise(x => x.Elapsed += null, (EventArgs)null);

            // Notify should still have been called once.
            _emailNotifierMock.Verify(x => x.Notify(It.Is<List<Basket>>(x => x.Count == 1 && x[0].Item.ItemId == 1)), Times.Once);

            _tooGoodToGoApiServiceMock.Setup(x => x.GetFavoriteBaskets(It.IsAny<string>(), It.IsAny<int>())).Returns(GetBasketsResponse(1));
            _timerMock.Raise(x => x.Elapsed += null, (EventArgs)null);

            // Notify should have been called a second time.
            _emailNotifierMock.Verify(x => x.Notify(It.Is<List<Basket>>(x => x.Count == 1 && x[0].Item.ItemId == 1)), Times.Exactly(2));

            static GetBasketsResponse GetBasketsResponse(int availableItems)
            {
                return new GetBasketsResponse
                {
                    Items = new List<Basket>
                    {
                        new Basket
                        {
                            DisplayName = "Basket N°1",
                            AvailableItems = availableItems,
                            Item = new Item
                            {
                                ItemId = 1
                            }
                        }
                    }
                };
            }
        }
    }
}
