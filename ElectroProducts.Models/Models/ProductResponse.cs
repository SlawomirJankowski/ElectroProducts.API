namespace ElectroProducts.Models.Domains
{
    public class ProductResponse
    {
        public string? SKU { get; set; }
        public string? Name { get; set; }
        public string? EAN { get; set; }
        public string? ProducerName { get; set; }
        public string? Category { get; set; }
        public string? PhotoUrl { get; set; }
        public decimal? StockQuantity { get; set; }
        public string? LogisticUnit { get; set; }
        public decimal? LogisticUnitPrice { get; set; }
        public decimal? DeliveryCost { get; set; }
    }

}
