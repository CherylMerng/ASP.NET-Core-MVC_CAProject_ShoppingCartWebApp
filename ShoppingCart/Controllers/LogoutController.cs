using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;
using ShoppingCart.Data;

namespace ShoppingCart.Controllers
{
    public class LogoutController : Controller
    {
        private Database db;

        public LogoutController(IConfiguration cfg)
        {
            db = new Database(cfg.GetConnectionString("db_conn"));
        }

        public IActionResult Index()
        {
            if (Request.Cookies["SessionId"] != null)
            {
                db.RemoveSession(Request.Cookies["SessionId"]);
                Response.Cookies.Delete("SessionId");
                Response.Cookies.Delete("isLoggedIn");
            }

            return RedirectToAction("Index", "Login");

        }
    }
}


