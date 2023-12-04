using CsvHelper.Configuration;

namespace RestApiProject.Models.Maps
{
    public sealed class InventoryMap : ClassMap<Inventory>
    {
        public InventoryMap()
        {
            //Mapping
            Map(m => m.SKU).Name("sku");
            Map(m => m.Unit).Name("unit");
            Map(m => m.Qty).Name("qty");
            Map(m => m.Shipping).Name("shipping");
            Map(m => m.ShippingCost).Name("shipping_cost");
        }
    }
}

