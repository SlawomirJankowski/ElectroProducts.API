using ElectroProducts.Models.Models;

namespace ElectroProducts.Models.Domains
{
    public class Price : IHasSku
    {
        public long Id { get; set; }
        public required string SKU { get; set; }
        public decimal? LogisticUnitPrice { get; set; }

    }

}
