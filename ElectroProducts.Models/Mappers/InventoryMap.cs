using CsvHelper.Configuration;
using ElectroProducts.Models.Domains;

namespace ElectroProducts.Models.Mappers
{
    public class InventoryMap : ClassMap<InventoryDTO>
    {
        public InventoryMap()
        {
            Map(m => m.SKU).Name("sku").Index(1);
            Map(m => m.LogisticUnit).Name("unit").Index(2);
            Map(m => m.StockQuantity).Name("qty").Index(3);
            Map(m => m.Shipping).Name("shipping").Index(7);
            Map(m => m.ShippingCost).Name("shipping_cost").Index(8);
        }
    }
}
