using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RemoteLab.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index()
        {
            Exception ex = Server.GetLastError();
            ViewBag.Message = "An Internal program error has occured.";
            return View("Index", ex);
        }

        // GET: Error/notfound
        public ActionResult NotFound()
        {
            Exception ex = Server.GetLastError();
            ViewBag.Message = "The requested resource could not be found.";
            return View("Index", ex);
        }

        // GET: Error/notallowed
        public ActionResult NotAllowed()
        {
            Exception ex = Server.GetLastError();
            ViewBag.Message = "You are not permitted to access the requested resource.";
            return View("Index", ex);
        }

    }
}