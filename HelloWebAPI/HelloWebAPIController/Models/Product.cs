using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelloWebAPIController.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public ProductElement Element { get; set; }
    }

    public class ProductElement
    {
        public int Num { get; set; }
        public String ElementName { get; set; }
    }
}