using System;

namespace TooGoodToGo.Api.Models
{
    public class TgtgBasket
    {
        public TgtgItem Item { get; set; }

        public TgtgStore Store { get; set; }

        public string DisplayName { get; set; }

        public TgtgPickupInterval PickupInterval { get; set; }

        public TgtgPickupLocation PickupLocation { get; set; }

        public DateTime PurchaseEnd { get; set; }

        public int ItemsAvailable { get; set; }

        public DateTime SoldOutAt { get; set; }

        public float Distance { get; set; }

        public bool Favorite { get; set; }

        public bool InSalesWindow { get; set; }

        public bool NewItem { get; set; }
    }
}
