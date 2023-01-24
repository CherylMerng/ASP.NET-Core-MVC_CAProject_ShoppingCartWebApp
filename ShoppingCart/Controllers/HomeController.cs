using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;
using ShoppingCart.Data;
using System;
using System.Linq;
using System.Data.SqlClient;
using System.Text.Json;

namespace ShoppingCart.Controllers;

public class HomeController : Controller
{
    private Database db;
    private CartDB cartdb;
    private PurchaseDB purchasedb;
    private RatingDB ratingdb;
    public int totalItem; 

    public HomeController (IConfiguration cfg)
    {
        db = new Database(cfg.GetConnectionString("db_conn"));
        cartdb = new CartDB(cfg.GetConnectionString("db_conn"));
        purchasedb = new PurchaseDB(cfg.GetConnectionString("db_conn"));
        ratingdb = new RatingDB(cfg.GetConnectionString("db_conn"));
    }

    //====================================================HOME===================================================================
    public IActionResult Index(string searchStr, string ProductId)
    {
        string sessionId;

        if (Request.Cookies["SessionId"] == null)
        {
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.MaxValue;

            sessionId = Guid.NewGuid().ToString();
            Response.Cookies.Append("SessionId", sessionId, options);
        }
        else
        {
            sessionId = Request.Cookies["SessionId"];
        }

        User user = db.GetUserBySession(sessionId);

        string username = "";
        string isLoggedIn;

        if (user == null)
        {
            username = sessionId;
            isLoggedIn = "false";
        }
        else
        {
            username = user.Username;
            isLoggedIn = "true";
        }

        Response.Cookies.Append("isLoggedIn", isLoggedIn);
        setCartQuantity(username);

        if (searchStr == null)
        {
            searchStr = "";
        }

        List<Product> products = db.SearchProduct(searchStr, searchStr);        

        if (HttpContext.Session.GetInt32("totalItem") == null)
        {
            this.totalItem = 0;

        }
        else
        {
            this.totalItem = (int)HttpContext.Session.GetInt32("totalItem");
        }

        ViewData["username"] = username;
        ViewData["isLoggedIn"] = isLoggedIn;
        ViewData["searchStr"] = searchStr;
        ViewData["products"] = products;
        ViewData["totalItem"] = this.totalItem;

        return View();

    }

    public string clickAddtoCart(string product)
    {
        string Username;
        int ProductId = Convert.ToInt32(product);

        string sessionid = Request.Cookies["SessionId"];
        string isLoggedIn = Request.Cookies["isLoggedIn"];

        if (isLoggedIn == "true")
        {
            User user = db.GetUserBySession(sessionid);
            Username = user.Username;
        }
        else
        {
            Username = sessionid;
        }

        if (cartdb.CheckCart(Username, ProductId) == false)
        {
            cartdb.InsertToCart(Username, ProductId);
        } else
        {
            cartdb.AddToCart(Username, ProductId);
        }

        //get total quantity for this username 

        return JsonSerializer.Serialize("true");
    }

    public void setCartQuantity(String Username)
    {
        this.totalItem = cartdb.CartQuantity(Username);

        HttpContext.Session.SetInt32("totalItem", this.totalItem);
    }

    //====================================================CART===================================================================
    public IActionResult Cart()
    {
        string username = "";
        string sessionid = Request.Cookies["SessionId"];
        string isLoggedIn = Request.Cookies["isLoggedIn"];

        if (isLoggedIn == null)
        {
            return RedirectToAction("Index", "Login");
        }
        else if (isLoggedIn == "true")
        {
            User user = db.GetUserBySession(sessionid);
            username = user.Username;
        }
        else if (isLoggedIn == "false")
        {
            username = sessionid;
        }

        List<Cart> Cartlist = cartdb.RetrieveCart(username);
        ViewData["CartList"] = Cartlist;
        ViewData["isLoggedIn"] = isLoggedIn;

        return View();
    }

    public void UpdateQuantity(string productid, string quantity)
    {
        string username = "";
        string sessionid = Request.Cookies["SessionId"];
        string isLoggedIn = Request.Cookies["isLoggedIn"];


        if (isLoggedIn == "true")
        {
            User user = db.GetUserBySession(sessionid);
            username = user.Username;
        }
        else
        {
            username = sessionid;
        }
        
        cartdb.UpdateCart(username, productid, quantity);
    }

    //====================================================PURCHASE===================================================================
    public IActionResult Purchase()
    {
        string username = "";
        string sessionid = Request.Cookies["SessionId"];
        string isLoggedIn = Request.Cookies["isLoggedIn"];

        if (isLoggedIn == "true")
        {
            User user = db.GetUserBySession(sessionid);
            username = user.Username;
        }
        else
        {
            username = sessionid;
        }

        List<Cart> Cart = new List<Cart>();
        Cart = cartdb.RetrieveCart(username);

        if (Cart.Count > 0)
        {
            if (isLoggedIn == "true")
            {
                purchasedb.AddToPurchase(username, username);
                cartdb.ClearCart(username);

                return RedirectToAction("ViewPurchase", "Home", new { username = username });
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        else
        {
            return RedirectToAction("Cart", "Home");
        }          
    }

    // From Server (redirected from the Home/Index page when GuestUser checkout cart and login to account)
    public IActionResult PurchaseAfterLogin(string username, string cartUser)
    {
        //add variables to addtopurchasemethod.
        purchasedb.AddToPurchase(username, cartUser);
        //clear cart from the fake user so next time if same person on the browser as fake user access the webpage of home bookmark, don't see the same cart,
        cartdb.ClearCart(cartUser);

        return RedirectToAction("ViewPurchase", "Home", new { username = username });
    }

    public IActionResult ViewPurchase(string username)
    {
        IEnumerable<OrderDetails> Purchase = new List<OrderDetails>();
        Purchase = purchasedb.RetrievePurchase(username);
        
        foreach (var order in Purchase)
        {
            if (!ratingdb.CheckPersonalRating(order.ProductId, username, order.PurchaseDate))
            {
                order.Rating = "0";
            }
            else
            {
                order.Rating = ratingdb.RetrievePersonalRating(order.ProductId, username, order.PurchaseDate);
            }
        }

        ViewData["username"] = username;
        ViewData["Purchase"] = Purchase;

        return View();
    }

    //==================================================RATING===================================================================
    public IActionResult SubmitRating(int rating, int ProductId, string username, string purchaseDate)
    {
        if (rating == 0)
        {
            return Content("User to try again");
        }
        ratingdb.AddIntoPersonalRating(rating, ProductId, username, purchaseDate);
        ratingdb.UpdateProductRating(ProductId);
        return RedirectToAction("ViewPurchase", "Home");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

