using System.Data.SqlClient;

namespace ShoppingCart.Data
{
    public class RatingDB
    {
        private string connStr;

        public RatingDB(string connStr)
        {
            this.connStr = connStr;
        }

        public bool AddIntoPersonalRating(int Rating, int ProductId, string username, string purchaseDate)
        {
            bool NeedUpdate = CheckPersonalRating(ProductId, username, purchaseDate);
            bool status = false;
            if (NeedUpdate)
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string q = String.Format(@"UPDATE PersonalRating SET Rating={0} WHERE Username ='{1}'AND PurchaseDate='{2}'AND ProductId ={3}", Rating, username, purchaseDate, ProductId);

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
            else
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string q = String.Format(@"INSERT INTO PersonalRating VALUES('{0}',{1},{2},'{3}')", purchaseDate, ProductId, Rating, username);

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

        }

        public bool CheckPersonalRating(int ProductId, string username, string purchaseDate)
        {
            bool status = false;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"SELECT * FROM PersonalRating
                WHERE Username = '{0}' AND ProductId = {1} AND PurchaseDate = '{2}'", username, ProductId, purchaseDate);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            status = true;
                        }
                        else
                        {
                            status = false;
                        }
                    }
                }
                conn.Close();
            }
            return status;
        }

        public Dictionary<int, int> RetrieveAverageRating(int ProductId)
        {
            Dictionary<int, int> ProductToRating = new Dictionary<int, int>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"SELECT ProductId,AVG(Rating) FROM PersonalRating GROUP BY ProductId");

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProductToRating.Add(reader.GetInt32(0), reader.GetInt32(1));

                        }
                    }
                }

                conn.Close();
            }
            return ProductToRating;

        }

        public bool UpdateProductRating(int ProductId)
        {
            bool status = false;
            Dictionary<int, int> ProductToRating = RetrieveAverageRating(ProductId);
            foreach (var dictionary in ProductToRating)
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string q = String.Format(@"UPDATE Product SET AvgRating = {0} WHERE ProductId ={1}", dictionary.Value, dictionary.Key);

                    using (SqlCommand cmd = new SqlCommand(q, conn))
                    {
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            status = true;
                        }
                    }

                    conn.Close();
                }

            }
            return status;
        }

        public string RetrievePersonalRating(int ProductId, string username, string purchaseDate)
        {
            string rating = "0";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string q = String.Format(@"SELECT Rating FROM PersonalRating
                WHERE Username = '{0}' AND ProductId = {1} AND PurchaseDate = '{2}'", username, ProductId, purchaseDate);

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            rating = reader.GetInt32(0).ToString();
                        }
                    }
                }
                conn.Close();
            }
            return rating;
        }
    }
}
