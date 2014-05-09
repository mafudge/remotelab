using RemoteLab.Models;
using RemoteLab.Services;
using RemoteLab.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RemoteLab.Controllers
{
    public class HomeController : Controller
    {
        private const string REZ_CLEARED_ONCE = "CLEARED-ONCE";

        private readonly RemoteLabService Svc;

        public HomeController(RemoteLabService Svc)
        {
            this.Svc = Svc;
        }


        // GET  /
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Index()
        {
            var rvm = await Svc.PopulateRemoteLabViewModelAsync(Properties.Settings.Default.DefaultPool, HttpContext.User.Identity.Name, Properties.Settings.Default.CleanupInMinutes);
            TempData["rvm"] = rvm;
            switch (rvm.ReservationStatus)
            {
                case ReservationStatus.ExistingReservation:
                    return RedirectToAction("ExistingRez");
                    break;
                case ReservationStatus.NewReservation:
                    return RedirectToAction("NewRez");
                    break;
                case ReservationStatus.PoolFull:
                    return RedirectToAction("PoolFull");
                    break;
                case ReservationStatus.Unknown:
                    //TODO: fix
                    throw new NotImplementedException();
                    break;
            }

            return View();
        }

        // GET /ExistingRez
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> ExistingRez() 
        {
            var rvm = (RemoteLabViewModel)TempData["rvm"];
            if (rvm == null || rvm.ReservationStatus != ReservationStatus.ExistingReservation) return RedirectToAction("Index");

            TempData["computerReservation"] = rvm.RemoteLabComputer.ComputerName;

            ViewBag.ClearedOnce = (HttpContext.Session[REZ_CLEARED_ONCE] != null);

            return View(rvm);
        }

        // GET /ExistingRez
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExistingRez(FormCollection form)
        {
            String ComputerReservation = form["ComputerReservation"];
            ReservationStatus RezStatus = EnumUtils.ParseEnum<ReservationStatus>(form["ReservationStatus"]);  //(ReservationStatus)(Enum.Parse(typeof(ReservationStatus),form["ReservationStatus"],true));

            if (RezStatus != ReservationStatus.ExistingReservation) return RedirectToAction("Index");

            //Clear Reservation, and reboot the computer
            await Svc.ClearReservationAsync(ComputerReservation);
            bool RebootResult = await Svc.RebootComputerAsync(ComputerReservation, HttpContext.User.Identity.Name, System.DateTime.Now);

            // Set a session cookie to avoid someone over-clearing reservations
            HttpContext.Session[REZ_CLEARED_ONCE] = REZ_CLEARED_ONCE;

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> NewRez() 
        {
            RemoteLabViewModel rvm = (RemoteLabViewModel) TempData["rvm"];

            if (rvm == null || rvm.ReservationStatus != ReservationStatus.NewReservation) return RedirectToAction("Index");

            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NewRez(FormCollection form) 
        {
            bool success = false;
            int result = 0;
            bool rebootResult = false;
            RemoteLabViewModel rvm = new RemoteLabViewModel();
            do {

                rvm = await Svc.PopulateRemoteLabViewModelAsync(Properties.Settings.Default.DefaultPool, HttpContext.User.Identity.Name, Properties.Settings.Default.CleanupInMinutes);

                if (rvm == null || rvm.ReservationStatus != ReservationStatus.NewReservation) return RedirectToAction("Index");

                success = await Svc.CheckRdpPortAndRebootIfUnresponsiveAsync(rvm.RemoteLabComputer.ComputerName, Properties.Settings.Default.ActiveDirectoryFqdn, 
                    rvm.CurrentUser, Properties.Settings.Default.RemoteDesktopProtocolTcpPort);

            } while (!success);
            // when you make it here, you have a valid computer, so make the reservation
            await Svc.MakeReservationAsync(rvm.RemoteLabComputer.ComputerName, rvm.CurrentUser);

            return View();
        }

        public async Task<ActionResult> PoolFull() 
        {
            RemoteLabViewModel rvm = (RemoteLabViewModel) TempData["rvm"];
            
            if (rvm == null || rvm.ReservationStatus != ReservationStatus.PoolFull) return RedirectToAction("Index");

            await Svc.LogAndEmailPoolFullEventAsync(rvm.Pool.ToLowerInvariant(), "N/A", rvm.CurrentUser, System.DateTime.Now);

            return View();
            
        }


    }
}