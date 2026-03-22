using ElectroProducts.Models.Models;

namespace ElectroProducts.Models.Domains
{
    public class Inventory : IHasSku
    {
        public long Id { get; set; }
        public required string SKU { get; set; }
        public string? LogisticUnit { get; set; }
        public decimal? StockQuantity { get; set; }
        public decimal? ShippingCost { get; set; }
    }

}
