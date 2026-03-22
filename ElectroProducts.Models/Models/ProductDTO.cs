namespace ElectroProducts.Models.Domains
{
    public class ProductDTO : Product
    {
        public bool? IsWire { get; set; }
        public string? Shipping { get; set; }

        public Product ToProduct() => new Product
        {
            Id = Id,
            SKU = SKU,
            Name = Name,
            EAN = EAN,
            ProducerName = ProducerName,
            Category = Category,
            PhotoUrl = PhotoUrl
        };
    }

}
