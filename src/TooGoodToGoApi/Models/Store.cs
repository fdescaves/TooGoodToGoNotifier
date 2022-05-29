namespace TooGoodToGo.Api.Models
{
    public class Store
    {
        public string StoreId { get; set; }

        public string StoreName { get; set; }

        public string Branch { get; set; }

        public string Description { get; set; }

        public string TaxIdentifier { get; set; }

        public string Website { get; set; }

        public StoreLocation StoreLocation { get; set; }

        public LogoPicture LogoPicture { get; set; }

        public string StoreTimeZone { get; set; }

        public bool Hidden { get; set; }

        public int FavoriteCount { get; set; }

        public bool WeCare { get; set; }

        public float Distance { get; set; }

        public CoverPicture CoverPicture { get; set; }

        public bool UsesEcommerceModel { get; set; }
    }
}
