using ElectroProducts.Models.Models;

namespace ElectroProducts.Models.Domains
{
    public class Product : IHasSku
    {
        public long Id { get; set; }
        public required string SKU { get; set; }
        public string? Name { get; set; }
        public string? EAN { get; set; }
        public string? ProducerName { get; set; }
        public string? Category { get; set; }
        public string? PhotoUrl { get; set; }
    }

}
