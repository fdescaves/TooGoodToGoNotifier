using System;
using System.Collections.Generic;

namespace TooGoodToGoNotifier
{
    public class Context
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public int UserId { get; set; }

        public Dictionary<int, bool> NotifiedBaskets { get; } = new Dictionary<int, bool>();
    }
}
