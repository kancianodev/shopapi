using CsvHelper.Configuration;

namespace RestApiProject.Models.Maps
{
    public sealed class ProductMap : ClassMap<Product>
    {
        public ProductMap()
        {
            //Mapping 
            Map(m => m.SKU).Name("SKU");
            Map(m => m.Name).Name("name");
            Map(m => m.EAN).Name("EAN");
            Map(m => m.ProducerName).Name("producer_name");
            Map(m => m.Category).Name("category");
            Map(m => m.IsWire).Name("is_wire");
            Map(m => m.Available).Name("available");
            Map(m => m.IsVendor).Name("is_vendor");
            Map(m => m.DefaultImage).Name("default_image");
        }
    }
}