using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RemoteLab.Models;
using RemoteLab.Services;
using RemoteLab.Utilities;
using System.Security.Claims;

namespace RemoteLab.Controllers
{
    public class PoolsController : Controller
    {
        private RemoteLabContext db = new RemoteLabContext();

        public RemoteLabService Svc {get; private set; }

        public PoolsController(RemoteLabService Svc)
        {
            this.Svc = Svc;
        }

        // GET: Pools
        [Authorize]
        public async Task<ActionResult> Index()
        {
            return View(this.Svc.GetPoolSummaryByAdminClaims((ClaimsPrincipal)HttpContext.User, Properties.Settings.Default.AdministratorADGroup).OrderBy( s=> s.PoolName));
        }

        // GET: Pools/Dashboard/PoolName
        [PoolAdministratorAuthorize]
        public async Task<ActionResult> Dashboard(string id)
        {
            if (id == null) { return new HttpStatusCodeResult(HttpStatusCode.BadRequest); }

            var stats = await Svc.GetPoolSummaryAsync(PoolName:id);
            if (stats == null) { return HttpNotFound(); }

            var Computers = await Svc.GetComputersByPoolNameAsync(PoolName:id);

            ViewBag.CurrentPool = id;
            ViewBag.Available = stats.PoolAvailable;
            ViewBag.Total = stats.PoolCount;
            ViewBag.InUse = stats.PoolInUse;
            return View(Computers.OrderBy( c=> c.ComputerName ));

        }

        // GET: Pools/Create
        [AdministratorAuthorize]
        public ActionResult Create()
        {            
            return View(new Pool() { RdpTcpPort=3389, CleanupInMinutes=30 });
        }

        // POST: Pools/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdministratorAuthorize]
        public async Task<ActionResult> Create([Bind(Include = "PoolName,ActiveDirectoryUserGroup,Logo,ActiveDirectoryAdminGroup,EmailNotifyList,RdpTcpPort,CleanupInMinutes")] Pool pool)
        {
            if (ModelState.IsValid)
            {
                await this.Svc.AddPoolAsync(pool);
                return RedirectToAction("Index");
            }

            return View(pool);
        }

        // GET: Pools/Edit/5
        [PoolAdministratorAuthorize]
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pool pool = await this.Svc.GetPoolByIdAsync(PoolName:id);
            if (pool == null)
            {
                return HttpNotFound();
            }
            return View(pool);
        }

        // POST: Pools/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PoolAdministratorAuthorize]
        public async Task<ActionResult> Edit([Bind(Include = "PoolName,ActiveDirectoryUserGroup,Logo,ActiveDirectoryAdminGroup,EmailNotifyList,RdpTcpPort,CleanupInMinutes")] Pool pool)
        {
            if (ModelState.IsValid)
            {
                await this.Svc.UpdatePoolAsync(pool);
                return RedirectToAction("Index");
            }
            return View(pool);
        }

        // GET: Pools/Delete/5
        [AdministratorAuthorize]
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pool pool = await this.Svc.GetPoolByIdAsync(PoolName:id);
            if (pool == null)
            {
                return HttpNotFound();
            }
            return View(pool);
        }

        // POST: Pools/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdministratorAuthorize]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            await this.Svc.RemovePoolByIdAsync(PoolName:id);

            return RedirectToAction("Index");

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Svc.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
