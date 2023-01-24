using ShoppingCart.Models;
using System.Data.SqlClient;

namespace ShoppingCart.Data
{
    public class CartDB
    {
        private string connStr;

        public CartDB(string connStr)
        {
            this.connStr = connStr;
        }

        public List<Cart> CartList(string Username, int ProductId)
        {
            List<Cart> cart = new List<Cart>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = string.Format(@$"SELECT u.Username,[ProductId],[Quantity] FROM [User] u INNER JOIN Cart c
                                        ON u.Username = c.Username WHERE u.Username = '{Username}' AND c.ProductId = {ProductId}");

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Cart cartItem = new Cart()
                            {
                                ProductId = (int)reader["ProductId"],
                                Username = (string)reader["Username"],
                                Quantity = (int)reader["Quantity"]
                            };

                            cart.Add(cartItem);
                        }

                    }
                }
                conn.Close();
            }


            return cart;
        }

        public int CartQuantity(string Username)
        {
            int Quantity = 0;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = string.Format(@$"Select SUM(Quantity) as Total from Cart where Username = '{Username}' group by Username");

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Quantity = (int)reader["Total"];
                        }

                    }
                }
                conn.Close();
            }


            return Quantity;
        }

        public bool CheckCart(string Username, int ProductId)
        {
            bool status = true;
            string user = "";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                //If the row doesn't exist. Insert new row.
                string q = string.Format($@"SELECT Username FROM Cart WHERE Username = '{Username}' AND ProductId = {ProductId}");

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            status = false;
                            return status;
                        }
                    }
                }
            }
            return status;
        }

        public bool InsertToCart(string Username, int ProductId)
        {
            bool status = false;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                //If the row doesn't exist. Insert new row.
                string q = string.Format($@"INSERT INTO Cart Values ('{Username}', {ProductId}, 1);");

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        status = true;
                    }
                }
            }
            return status;
        }
        public bool AddToCart(string Username, int ProductId)
        {
            bool status = false;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                //If the row doesn't exist. Insert new row.
                string q = string.Format($@"UPDATE Cart SET Quantity= Quantity+1 
                                       WHERE Username = '{Username}' AND ProductId = {ProductId}");

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        status = true;
                    }
                }
            }
            return status;
        }

        public List<Cart> RetrieveCart(string Username)
        {
            List<Cart> cart = new List<Cart>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = string.Format(@"SELECT Cart.ProductId, Cart.Quantity, Product.Price, Product.Name, Product.Description FROM Cart INNER JOIN Product ON Cart.ProductId = Product.ProductId WHERE Username = '{0}'", Username);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Cart cartItem = new Cart()
                            {
                                ProductId = (int)reader["ProductId"],
                                Quantity = (int)reader["Quantity"],
                                Price = (int)reader["Price"],
                                Name = (string)reader["Name"],
                                Description = (string)reader["Description"]
                            };

                            cart.Add(cartItem);
                        }

                    }
                }
                conn.Close();
            }


            return cart;
        }
        public bool UpdateCart(string Username, string ProductId, string quantity)
        {
            bool status = false;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                //If the row doesn't exist. Insert new row.
                int PId = Convert.ToInt32(ProductId);
                int Quant = Convert.ToInt32(quantity);
                string q = "";

                if (quantity == "0")
                {
                    q = string.Format($@"DELETE FROM Cart WHERE Username = '{Username}' AND ProductId = {ProductId}");
                }
                else
                {
                    q = string.Format($@"UPDATE Cart SET Quantity= {Quant} WHERE Username = '{Username}' AND ProductId = {ProductId}");
                }

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        status = true;
                    }
                }
                conn.Close();
            }
            return status;
        }
        public bool TransferPurchase(string Username)
        {
            bool status = false;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                Guid orderid = Guid.NewGuid();
                DateTime today = DateTime.Today;
                string date = today.ToString("dd/MM/yyyy");

                string q = string.Format($@"INSERT INTO OrderList VALUES ('{orderid}', '{Username}', '{date}')");

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        status = true;
                    }
                }

                List<Cart> cartpurchase = new List<Cart>();
                q = string.Format($@"SELECT * FROM Cart WHERE Username = '{Username}'");

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if ((int)reader["Quantity"] != 0)
                            {
                                Cart cartItem = new Cart()
                                {
                                    Username = (string)reader["Username"],
                                    ProductId = (int)reader["ProductId"],
                                    Quantity = (int)reader["Quantity"],
                                };

                                cartpurchase.Add(cartItem);
                            }
                        }
                    }
                }

                q = string.Format($@"DELETE FROM Cart WHERE Username = '{Username}'");

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        status = true;
                    }
                }

                foreach (Cart cart in cartpurchase)
                {
                    for (int x = 0; x < cart.Quantity; x++)
                    {
                        Guid guid = Guid.NewGuid();
                        q = string.Format($@"INSERT INTO OrderDetails VALUES ('{guid}', '{orderid}', {cart.ProductId})");

                        using (SqlCommand cmd = new SqlCommand(q, conn))
                        {
                            if (cmd.ExecuteNonQuery() == 1)
                            {
                                status = true;
                            }
                        }
                    }
                }
                conn.Close();

                return status;
            }
        }

        public bool ClearCart(string username)
        {
            bool status = false;
            List<Cart> Cart = new List<Cart>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"DELETE FROM Cart
                WHERE Username = '{0}'", username);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    cmd.ExecuteNonQuery();

                }

                Cart = RetrieveCart(username);
                if (Cart.Count == 0)
                {
                    return true;
                }
                conn.Close();
            }

            return status;
        }
    }
}
