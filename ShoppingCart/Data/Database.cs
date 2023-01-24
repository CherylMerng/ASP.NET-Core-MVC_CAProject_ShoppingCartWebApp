using System;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Hosting.Server;
using ShoppingCart.Models;

namespace ShoppingCart.Data
{
    public class Database
    {
        private string connStr;
        //private List<Cart> currentCart; 

        public Database(string connStr)
        {
            this.connStr = connStr;
            //this.currentCart = new List<Cart>();
        }

        public User GetUserBySession(string session)
        {
            User user = null;
            Guid sessionId = new Guid(session);
          
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                
                string q = string.Format(@"SELECT u.Username, Password
             FROM Session s, [User] u
                WHERE s.Username = u.Username AND 
                    s.SessionId = '{0}'", sessionId);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Username = reader.GetString(0),
                                Password = reader.GetString(1)
                            };
                        }
                    }
                }

                conn.Close();
            }

            return user;
        }

        public User GetUserByUsername(string username)
        {
            User user = null;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"SELECT * FROM [User] 
                WHERE Username = '{0}'", username);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Username = reader.GetString(0),
                                Password = reader.GetString(1)
                            };
                        }
                    }
                }

                conn.Close();
            }

            return user;
        }

        public string AddSession(string username)
        {
            string sessionId = null;
            Guid guid = Guid.NewGuid();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = string.Format(@"INSERT INTO Session(
                SessionId, Username, Timestamp) VALUES('{0}', '{1}', {2})",
                        guid, username, DateTimeOffset.Now.ToUnixTimeSeconds());

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        sessionId = guid.ToString();
                    }
                }

                conn.Close();
            }

            return sessionId;
        }

        public bool RemoveSession(string session)
        {
            bool status = false;
            Guid sessionId = new Guid(session);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"DELETE FROM Session
                WHERE sessionId = '{0}'", sessionId);

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

        public bool RemoveSessionByUser(string username)
        {
            bool status = false;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"DELETE FROM Session
                WHERE Username = '{0}'", username);

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

        public List<Product> SearchProduct(string Name,string Description)
        {
            List<Product> products = new List<Product>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                // query
                string q = string.Format(@"SELECT * FROM Product WHERE 
                [Name] LIKE '%{0}%' OR 
                [Description] LIKE '%{1}%'",
                        Name, Description);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Product product = new Product
                            {
                                ProductId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Price = reader.GetInt32(2),
                                Description = reader.GetString(3),
                                AvgRating = reader.GetInt32(4).ToString()
                            };

                            if (product.AvgRating == null)
                            {
                                product.AvgRating = "0";
                            }
                            else
                            {
                                product.AvgRating = reader.GetInt32(4).ToString();
                            };

                            products.Add(product);
                        }
                    }
                }

                conn.Close();
            }

            return products;
        }
        
    }
}