using System;
namespace ShoppingCart.Models
{
    public class OrderDetails
    {
        public List<Guid> ActivationCode { get; set; }
        public Guid OrderId { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Username { get; set; }
        public string PurchaseDate { get; set; }
        public int Quantity { get; set; }
        public string Rating { get; set; }
    }
}

