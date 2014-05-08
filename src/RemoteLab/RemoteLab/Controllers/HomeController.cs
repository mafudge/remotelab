using RemoteLab.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RemoteLab.Controllers
{
    public class HomeController : Controller
    {

        private readonly RemoteLabContext Db;

        public HomeController(RemoteLabContext Db)
        {
            this.Db = Db;
        }

        public ActionResult Index()
        {
            Db.Computers.ToList();
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}