using ShoppingCart.Models;
using System.Data.SqlClient;

namespace ShoppingCart.Data
{
    public class PurchaseDB
    {
        private string connStr;

        public PurchaseDB(string connStr)
        {
            this.connStr = connStr;
        }

        public IEnumerable<OrderDetails> RetrievePurchase(string username)
        {
            Dictionary<int, List<string>> ProductToDate = ProductToDates(username);
            List<OrderDetails> OrderInfo = new List<OrderDetails>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                foreach (var dictionary in ProductToDate)
                {
                    foreach (var date in dictionary.Value)
                    {
                        string q = String.Format(@"SELECT ActivationCode,PurchaseDate, Name, Description,OrderDetails.ProductId FROM OrderList, OrderDetails, Product WHERE OrderList.OrderId = OrderDetails.OrderId AND Username= '{0}'AND OrderDetails.ProductID = Product.ProductID AND OrderDetails.ProductID = {1} AND OrderList.PurchaseDate = '{2}'",
                   username, dictionary.Key, date);
                        List<Guid> Codes = new List<Guid>();

                        using (SqlCommand cmd = new SqlCommand(q, conn))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                OrderDetails OrderProduct = new OrderDetails { Quantity = 0, ActivationCode = Codes };

                                while (reader.Read())
                                {
                                    Codes.Add(reader.GetGuid(0));
                                    OrderProduct.PurchaseDate = reader.GetString(1);
                                    OrderProduct.Name = reader.GetString(2);
                                    OrderProduct.Description = reader.GetString(3);
                                    OrderProduct.ProductId = reader.GetInt32(4);

                                }

                                OrderInfo.Add(OrderProduct);

                            }

                        }

                    }

                }
                conn.Close();
            }
            IEnumerable<OrderDetails> OrderInformation = from order in OrderInfo orderby order.PurchaseDate descending select order;
            return OrderInformation;

        }


        public bool AddToPurchase(string username, string cartUser)
        {
            bool status = false;
            Guid orderId = Guid.NewGuid();
            string purchasedate = DateTime.Today.ToString("dd MMMM yyyy");
            Dictionary<int, int> ProductToQuantity = QuantityPerProduct(cartUser);
            //to retrieve the cart-> each product's quantity from the GuestUser side. before can insert to orderdetails of actual username.
            //insert into orderlist-> need insert into correct/actual username.
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"INSERT INTO OrderList (OrderId, Username, PurchaseDate) VALUES('{0}','{1}','{2}')",
                        orderId, username, purchasedate);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        status = true;
                    }
                }
                foreach (var dictionary in ProductToQuantity)
                {
                    var Quantity = dictionary.Value;

                    for (int i = 0; i < Quantity; i++)
                    {
                        string j = String.Format(@"INSERT INTO OrderDetails(ActivationCode, OrderId, ProductId) VALUES('{0}','{1}',{2})",
                        Guid.NewGuid(), orderId, dictionary.Key);
                        using (SqlCommand cmd = new SqlCommand(j, conn))
                        {
                            if (cmd.ExecuteNonQuery() == 1)
                            {
                                status = true;
                            }
                        }
                    }
                }

                conn.Close();
            }

            return status;

        }

        public Dictionary<int, int> QuantityPerProduct(string username)
        {
            Dictionary<int, int> ProductToQuantity = new Dictionary<int, int>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"SELECT ProductId,Quantity FROM Cart
                WHERE Username = '{0}'", username);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProductToQuantity.Add(reader.GetInt32(0), reader.GetInt32(1));

                        }
                    }
                }

                conn.Close();
            }
            return ProductToQuantity;

        }

        public Dictionary<int, List<string>> ProductToDates(string username)
        {
            Dictionary<int, List<string>> ProductToDates = new Dictionary<int, List<string>>();
            List<string> dates = new List<string>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"SELECT ProductId,PurchaseDate FROM OrderDetails, OrderList
                WHERE OrderDetails.OrderId = OrderList.OrderId and Username = '{0}'GROUP BY PurchaseDate,ProductId", username);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (ProductToDates.ContainsKey(reader.GetInt32(0)))
                            {
                                List<string> values = ProductToDates[reader.GetInt32(0)];
                                values.Add(reader.GetString(1));

                            }
                            else
                            {

                                ProductToDates.Add((reader.GetInt32(0)), new List<string>() { reader.GetString(1) });

                            }


                        }
                    }
                }

                conn.Close();
            }
            return ProductToDates;

        }
    }
}
