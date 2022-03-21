using System;

namespace TooGoodToGoNotifier.Api
{
    public class TooGoodToGoApiContext
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public int? UserId { get; set; }

        public DateTime? LastAuthenticatedOn { get; set; }
    }
}
