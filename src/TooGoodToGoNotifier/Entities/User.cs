using System;
using System.Collections.Generic;

namespace TooGoodToGoNotifier.Entities
{
    public class User
    {
        public Guid UserId { get; set; }

        public string Email { get; set; }

        public List<string> FavoriteBaskets { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
