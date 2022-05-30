using System;
using System.Collections.Generic;

namespace TooGoodToGoNotifier.Models
{
    public class User
    {
        public Guid UserId { get; set; }

        public string Email { get; set; }

        public List<Basket> FavoriteBaskets { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
