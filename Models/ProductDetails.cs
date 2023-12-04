namespace RestApiProject.Models
{
    public class ProductDetails
    {
        public string Name { get; set; }
        public string EAN { get; set; }
        public string ProducerName { get; set; }
        public string Category { get; set; }
        public string DefaultImage { get; set; }
        public int Qty { get; set; }
        public string Unit { get; set; }
        public decimal NettPrice { get; set; }
        public decimal ShippingCost { get; set; }
    }
}