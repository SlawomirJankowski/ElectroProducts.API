using CsvHelper.Configuration;
using ElectroProducts.Models.Domains;

namespace ElectroProducts.Models.Mappers
{
    public class PriceMap : ClassMap<Price>
    {
        public PriceMap()
        {
            Map(m => m.SKU).Index(1);
            Map(m => m.LogisticUnitPrice).Index(5);
        }
    }
}
