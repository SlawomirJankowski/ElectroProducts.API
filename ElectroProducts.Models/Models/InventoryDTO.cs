namespace ElectroProducts.Models.Domains
{
    public class InventoryDTO : Inventory
    {
        public string? Shipping { get; set; }

        public Inventory ToInventory() => new Inventory
        {
            Id = Id,
            SKU = SKU,
            LogisticUnit = LogisticUnit,
            StockQuantity = StockQuantity,
            ShippingCost = ShippingCost
        };
    }

}
