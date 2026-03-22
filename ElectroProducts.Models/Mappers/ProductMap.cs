using CsvHelper.Configuration;
using ElectroProducts.Models.Domains;

namespace ElectroProducts.Models.Mappers
{
    public class ProductMap : ClassMap<ProductDTO>
    {
        public ProductMap()
        {
            Map(m => m.SKU).Name("SKU").Index(1);
            Map(m => m.Name).Name("name").Index(2);
            Map(m => m.EAN).Name("EAN").Index(4);
            Map(m => m.ProducerName).Name("producer_name").Index(5);
            Map(m => m.Category).Name("category").Index(6);
            Map(m => m.IsWire).Name("is_wire").Index(7);
            Map(m => m.Shipping).Name("shipping").Index(8);
            Map(m => m.PhotoUrl).Name("default_image").Index(17);
        }
    }
}
