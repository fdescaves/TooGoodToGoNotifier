namespace TooGoodToGo.Api.Models
{
    public class Address
    {
        public Country Country { get; set; }

        public string AddressLine { get; set; }

        public string City { get; set; }

        public string PostalCode { get; set; }
    }
}
