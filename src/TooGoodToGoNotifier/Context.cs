using System.Collections.Generic;

namespace TooGoodToGoNotifier
{
    public class Context
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public int TooGoodToGoUserId { get; set; }

        public Dictionary<string, bool> NotifiedBaskets { get; } = new Dictionary<string, bool>();
    }
}
