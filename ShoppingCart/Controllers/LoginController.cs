using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;
using ShoppingCart.Data;


namespace ShoppingCart.Controllers
{
    public class LoginController : Controller
    {
        private Database db;

        public LoginController(IConfiguration cfg)
        {
            db = new Database(cfg.GetConnectionString("db_conn"));
        }

        public IActionResult Index(IFormCollection form)
        {
            string username = form["username"];
            string password = form["password"];

            string isLoggedIn = Request.Cookies["isLoggedIn"];

            if (username != null)
            {
                User user = db.GetUserByUsername(username);
                if (user != null)
                {
                    if (user.Password == password)
                    {                       
                        string oldsessionId = "";

                        if (isLoggedIn == "false")
                        {
                            oldsessionId = Request.Cookies["SessionId"];
                        }

                        if(Request.Cookies["SessionId"] != null)
                        {
                            db.RemoveSession(Request.Cookies["SessionId"]);
                        }
                        
                        db.RemoveSessionByUser(username);

                        string sessionId = db.AddSession(user.Username);
                        Response.Cookies.Append("SessionId", sessionId);

                        if (isLoggedIn == "false")
                        {
                            Response.Cookies.Append("isLoggedIn", "true");

                            return RedirectToAction("PurchaseAfterLogin", "Home", new { username = username, cartUser = oldsessionId });
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }                        
                    }
                    else
                    {
                        ViewData["message"] = "Your username or password is incorrect. Please try again.";
                        return View();
                    }
                }
                else
                {
                    ViewData["message"] = "Your username or password is incorrect. Please try again.";
                    return View();
                }
            }
            else
            {
                string usersession = Request.Cookies["SessionId"];
                if(usersession != null && isLoggedIn == "true")
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return View();
        }
    }
}