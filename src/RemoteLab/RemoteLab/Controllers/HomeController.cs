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
        private const string RVM = "rvm";
        private const string COMPUTER_RESERVATION = "ComputerReservation";


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
            TempData[RVM] = rvm;
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
            var rvm = (RemoteLabViewModel)TempData[RVM];
            if (rvm == null || rvm.ReservationStatus != ReservationStatus.ExistingReservation) return RedirectToAction("Index");

            TempData[COMPUTER_RESERVATION] = rvm.RemoteLabComputer.ComputerName;
            var stats = await Svc.GetPoolSummaryAsync(rvm.Pool.PoolName);
            ViewBag.ClearedOnce = (HttpContext.Session[REZ_CLEARED_ONCE] != null);
            ViewBag.Available = stats.PoolAvailable;
            ViewBag.Total = stats.PoolCount;
            ViewBag.InUse = stats.PoolInUse;

            return View(rvm);
        }

        // POST /ExistingRez (you clicked clear reservation)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ClearRez(FormCollection form)
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

        // GET /NewRez
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> NewRez() 
        {
            RemoteLabViewModel rvm = (RemoteLabViewModel) TempData[RVM];

            if (rvm == null || rvm.ReservationStatus != ReservationStatus.NewReservation) return RedirectToAction("Index");

            var stats = await Svc.GetPoolSummaryAsync(rvm.Pool.PoolName);
            ViewBag.Available = stats.PoolAvailable;
            ViewBag.Total = stats.PoolCount;
            ViewBag.InUse = stats.PoolInUse;

            return View();
        }

        // POST /NewRez (You clicked I agree, and must MakeRez)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MakeRez(FormCollection form) 
        {
            bool success = false;
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

            return RedirectToAction("Index");
        }

        // GET / PoolFull
        public async Task<ActionResult> PoolFull() 
        {
            RemoteLabViewModel rvm = (RemoteLabViewModel) TempData[RVM];
            
            if (rvm == null || rvm.ReservationStatus != ReservationStatus.PoolFull) return RedirectToAction("Index");

            await Svc.LogAndEmailPoolFullEventAsync(rvm.Pool.PoolName.ToLowerInvariant(), "N/A", rvm.CurrentUser, System.DateTime.Now);

            var stats = await Svc.GetPoolSummaryAsync(rvm.Pool.PoolName); 
            ViewBag.Available = stats.PoolAvailable;
            ViewBag.Total = stats.PoolCount;
            ViewBag.InUse = stats.PoolInUse;


            return View();
            
        }

        // GET / RDPFile returns reservation for valid user
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> RdpFile() 
        {
            var rezComputer = (String) TempData[COMPUTER_RESERVATION];
            if (rezComputer == null)  return RedirectToAction("Index");
            var rdpComputer = String.Format("{0}.{1}",rezComputer, Properties.Settings.Default.ActiveDirectoryFqdn).ToLowerInvariant();
            var userName = String.Format("{0}@{1}",HttpContext.User.Identity.Name, Properties.Settings.Default.ActiveDirectoryFqdn).ToLowerInvariant();
            var contentType = "application/rdp";
            var buff = Svc.GenerateRdpFileContents(Properties.Resources.RdpFileSettings, rdpComputer, userName);

            Response.AddHeader("Content-Disposition", "attachment; filename=RemoteLabReservation.rdp");
            
            return Content(buff, contentType, System.Text.Encoding.UTF8);
                
        }

    }
}