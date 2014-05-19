using RemoteLab.Models;
using RemoteLab.Services;
using RemoteLab.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
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
        private const string CHOSEN_POOL = "CHOSEN-POOL";
        private const string NO_POOLS = "NO-POOLS";
        private const string POOL_COUNT = "POOL-COUNT";


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
            var chosenPool = (String)HttpContext.Session[CHOSEN_POOL] ?? String.Empty;
            var rvm = await Svc.PopulateRemoteLabViewModelAsync(chosenPool, HttpContext.User.Identity.Name);
            TempData[RVM] = rvm;
            switch (rvm.ReservationStatus)
            {
                case ReservationStatus.NoPoolSelected:
                    return RedirectToAction("ChoosePool");
                case ReservationStatus.ExistingReservation:
                    return RedirectToAction("ExistingRez");
                case ReservationStatus.NewReservation:
                    return RedirectToAction("NewRez");
                case ReservationStatus.PoolFull:
                    return RedirectToAction("PoolFull");
                case ReservationStatus.Unknown:
                    //TODO: fix? redirect to nice error page?
                    throw new NotImplementedException();
            }

            return View();
        }

        // GET /ChoosePool
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> ChoosePool()
        {
            var userPools = this.Svc.GetPoolSummaryByUserClaims((ClaimsPrincipal)HttpContext.User);
            HttpContext.Session[POOL_COUNT] = userPools.Count();
            switch (userPools.Count())
            {
                case 0: // this user has no pools
                    TempData[NO_POOLS] = NO_POOLS;
                    return RedirectToAction("NoPools");
                case 1: //only 1 pool, no need to select
                    HttpContext.Session[CHOSEN_POOL] = userPools.FirstOrDefault().PoolName;
                    return RedirectToAction("Index");
                default: //more than one, make the user choose in the view
                    return View(userPools.OrderBy( s => s.PoolName));

            }
        }

        // GET /ChoosePool
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChoosePool(FormCollection form)
        {
            var PoolName = (String)form["PoolName"] ?? String.Empty;
            HttpContext.Session[CHOSEN_POOL] = PoolName;
            return RedirectToAction("Index");
        }

        // GET /ChooseDifferentPool
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> ChooseDifferentPool()
        {
            HttpContext.Session.Remove(CHOSEN_POOL);
            return RedirectToAction("Index");
        }
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> NoPools()
        {
            var nopools = (string) TempData[NO_POOLS] ?? String.Empty;
            if (String.IsNullOrEmpty(nopools))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }

        // GET /NewRez
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> NewRez()
        {
            RemoteLabViewModel rvm = (RemoteLabViewModel)TempData[RVM];

            if (rvm == null || rvm.ReservationStatus != ReservationStatus.NewReservation) return RedirectToAction("Index");

            var stats = await Svc.GetPoolSummaryAsync(rvm.Pool.PoolName);
            ViewBag.CurrentPool = rvm.Pool.PoolName;
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
            string PoolName = (string)HttpContext.Session[CHOSEN_POOL];
            RemoteLabViewModel rvm = new RemoteLabViewModel();
            do
            {
                rvm = await Svc.PopulateRemoteLabViewModelAsync(PoolName, HttpContext.User.Identity.Name);

                if (rvm == null || rvm.ReservationStatus != ReservationStatus.NewReservation) return RedirectToAction("Index");

                success = await Svc.CheckRdpPortAndRebootIfUnresponsiveAsync(rvm.RemoteLabComputer.ComputerName, Properties.Settings.Default.ActiveDirectoryFqdn,
                    rvm.CurrentUser, rvm.Pool.PoolName, rvm.Pool.RdpTcpPort);

            } while (!success);
            // when you make it here, you have a valid computer, so make the reservation
            await Svc.MakeReservationAsync(rvm.RemoteLabComputer.ComputerName, rvm.CurrentUser);

            return RedirectToAction("Index");
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
            ViewBag.CurrentPool = rvm.Pool.PoolName;
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
            String PoolName = (String)HttpContext.Session[CHOSEN_POOL];
            ReservationStatus RezStatus = EnumUtils.ParseEnum<ReservationStatus>(form["ReservationStatus"]); 

            if (RezStatus != ReservationStatus.ExistingReservation) return RedirectToAction("Index");

            //Clear Reservation, and reboot the computer
            await Svc.ClearReservationAsync(ComputerReservation);
            bool RebootResult = await Svc.RebootComputerAsync(ComputerReservation, HttpContext.User.Identity.Name, PoolName,  System.DateTime.Now);

            // Set a session cookie to avoid someone over-clearing reservations
            HttpContext.Session[REZ_CLEARED_ONCE] = REZ_CLEARED_ONCE;

            return RedirectToAction("Index");
        }


        // GET / PoolFull
        public async Task<ActionResult> PoolFull() 
        {
            RemoteLabViewModel rvm = (RemoteLabViewModel) TempData[RVM];
            
            if (rvm == null || rvm.ReservationStatus != ReservationStatus.PoolFull) return RedirectToAction("Index");

            await Svc.LogAndEmailPoolFullEventAsync(rvm.Pool.PoolName.ToLowerInvariant(), "N/A", rvm.CurrentUser, System.DateTime.Now);

            var stats = await Svc.GetPoolSummaryAsync(rvm.Pool.PoolName);
            ViewBag.CurrentPool = rvm.Pool.PoolName;
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
            var poolName = (String) HttpContext.Session[CHOSEN_POOL];
            var pool = await Svc.GetPoolByIdAsync(poolName);
            var rdpComputer = String.Format("{0}.{1}:{2}",rezComputer, Properties.Settings.Default.ActiveDirectoryFqdn, pool.RdpTcpPort).ToLowerInvariant();
            var userName = String.Format("{0}@{1}",HttpContext.User.Identity.Name, Properties.Settings.Default.ActiveDirectoryFqdn).ToLowerInvariant();
            var contentType = "application/rdp";
            var buff = Svc.GenerateRdpFileContents(Properties.Settings.Default.RdpFileSettings, rdpComputer, userName);

            Response.AddHeader("Content-Disposition", "attachment; filename=RemoteLabReservation.rdp");
            
            return Content(buff, contentType, System.Text.Encoding.UTF8);
                
        }

    }
}