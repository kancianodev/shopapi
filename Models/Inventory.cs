using System;
namespace RestApiProject.Models
{
    public class Inventory
    {
        public string SKU { get; set; }
        public int Qty { get; set; }
        public string Unit { get; set; }
        public string Shipping { get; set; }
        public decimal? ShippingCost { get; set; }
    }
}

