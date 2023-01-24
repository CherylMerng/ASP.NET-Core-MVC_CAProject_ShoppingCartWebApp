using System;
namespace ShoppingCart.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public string AvgRating { get; set; }
    }
}

