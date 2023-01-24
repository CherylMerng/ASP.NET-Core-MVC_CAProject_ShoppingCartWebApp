using System;
namespace ShoppingCart.Models
{
    public class Cart
    {
        public string Username { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } 
        public int Price { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AvgRating { get; set; }
    }
}

