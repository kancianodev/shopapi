﻿using System;
namespace RestApiProject.Models
{
    public class Product
    {
        public string SKU { get; set; }
        public string Name { get; set; }
        public string EAN { get; set; }
        public string ProducerName { get; set; }
        public string Category { get; set; }
        public bool IsWire { get; set; }
        public bool Available { get; set; }
        public bool IsVendor { get; set; }
        public string DefaultImage { get; set; }
    }
}

