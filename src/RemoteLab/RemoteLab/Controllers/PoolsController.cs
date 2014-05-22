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
using Effortless.Net.Encryption;

namespace RemoteLab.Controllers
{
    public class PoolsController : Controller
    {

        public RemoteLabService Svc {get; private set; }

        public PasswordUtility Pw { get; private set; }

        public PoolsController(RemoteLabService Svc, PasswordUtility Pw)
        {
            this.Svc = Svc;
            this.Pw = Pw;
        }

        // GET: Pools
        [Authorize]
        public async Task<ActionResult> Index()
        {
            return View(this.Svc.GetPoolSummaryByAdminClaims((ClaimsPrincipal)HttpContext.User, Properties.Settings.Default.AdministratorADGroup).OrderBy( s=> s.PoolName));
        }

        // GET: Pools/Events/PoolName
        [PoolAdministratorAuthorize]
        public async Task<ActionResult> Events(string id, string ComputerName="", string UserName="")        
        {
            if (id == null) { throw new HttpException((int)HttpStatusCode.BadRequest, "Missing PoolName"); }

            var stats = await Svc.GetPoolSummaryAsync(PoolName: id);
            if (stats == null) { throw new HttpException((int)HttpStatusCode.NotFound, "PoolName was Not found"); }

            var Events = await Svc.GetEventsAsync(PoolName:id);
            if (!String.IsNullOrEmpty(ComputerName))
            {
                Events = Events.Where( e=> e.ComputerName.Equals(ComputerName, StringComparison.InvariantCultureIgnoreCase));
            }
            if (!String.IsNullOrEmpty(UserName))
            {
                Events = Events.Where(e => e.UserName.Equals(UserName, StringComparison.InvariantCultureIgnoreCase));
            }

            ViewBag.CurrentPool = id;
            ViewBag.Available = stats.PoolAvailable;
            ViewBag.Total = stats.PoolCount;
            ViewBag.InUse = stats.PoolInUse;
            return View(Events.OrderByDescending( e => e.DtStamp ));
        }

        // GET: Pools/DownloadEvents/PoolName
        [PoolAdministratorAuthorize]
        [HttpGet]
        public async Task<ActionResult> DownloadEvents(string id)
        {
            if (id == null) { throw new HttpException((int)HttpStatusCode.BadRequest, "Missing PoolName"); }
            TempData["id"] = id;

            var stats = await Svc.GetPoolSummaryAsync(PoolName: id);
            if (stats == null) { throw new HttpException((int)HttpStatusCode.NotFound, "PoolName was Not found"); }

            ViewBag.CurrentPool = id;
            ViewBag.Available = stats.PoolAvailable;
            ViewBag.Total = stats.PoolCount;
            ViewBag.InUse = stats.PoolInUse;
            return View();
        }

        // POST: Pools/DownloadEvents/PoolName
        [PoolAdministratorAuthorize]
        [HttpPost]
        public async Task<ActionResult> DownloadEvents([Bind(Include = "PoolName,StartDate,EndDate,Format")] DownloadEventsViewModel devm)
        {
            
            if (devm.PoolName == null) { throw new HttpException((int)HttpStatusCode.BadRequest, "Missing PoolName"); }

            var stats = await Svc.GetPoolSummaryAsync(devm.PoolName);
            if (stats == null) { throw new HttpException((int)HttpStatusCode.NotFound, "PoolName was Not found"); }

            var downloadEvents= await this.Svc.GetEventsAsync(devm.PoolName, devm.StartDate, devm.EndDate);

            var buff = this.Svc.EventsToCsv(downloadEvents);
   
            var contentType = "text/csv";
            var fileName = String.Format("events-{0}-{1:yyyyMMdd}-{2:yyyyMMdd}.csv", devm.PoolName, devm.StartDate, devm.EndDate);
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName );


            return Content(buff, contentType, System.Text.Encoding.UTF8);            

        }

        // GET: Pools/DownloadScripts/PoolName
        [PoolAdministratorAuthorize]
        [HttpGet]
        public async Task<ActionResult> DownloadScripts(string id)
        {
            if (id == null) { throw new HttpException((int)HttpStatusCode.BadRequest, "Missing PoolName"); }
            TempData["id"] = id;

            var stats = await Svc.GetPoolSummaryAsync(PoolName: id);
            if (stats == null) { throw new HttpException((int)HttpStatusCode.NotFound, "PoolName was Not found"); }

            ViewBag.CurrentPool = id;
            ViewBag.Available = stats.PoolAvailable;
            ViewBag.Total = stats.PoolCount;
            ViewBag.InUse = stats.PoolInUse;
            return View();
        }

        // POST: Pools/DownloadScripts/PoolName
        [PoolAdministratorAuthorize]
        [HttpPost]
        public async Task<ActionResult> DownloadScripts(FormCollection from)
        {
            String PoolName = (String)TempData["id"];
            if (PoolName == null) { throw new HttpException((int)HttpStatusCode.BadRequest, "Missing PoolName"); }
            var stats = await Svc.GetPoolSummaryAsync(PoolName: PoolName);
            if (stats == null) { throw new HttpException((int)HttpStatusCode.NotFound, "PoolName was Not found"); }
            
            var buff= String.Format(Properties.Settings.Default.RemoteLabSettingsFileContent,
                            String.Format("{0};{1}","Provider=sqloledb",this.Svc.DatbaseConnectionString()),
                            stats.RemoteAdminUser, 
                            PoolName);
            var contentType = "text/plain";
            
            Response.AddHeader("Content-Disposition", "attachment; filename=RemoteLabSettings.vbs");

            return Content(buff, contentType, System.Text.Encoding.UTF8);

        }

        // GET: Pools/Dashboard/PoolName
        [PoolAdministratorAuthorize]
        public async Task<ActionResult> Dashboard(string id)
        {
            if (id == null) { throw new HttpException((int)HttpStatusCode.BadRequest, "Missing PoolName"); }
            var stats = await Svc.GetPoolSummaryAsync(PoolName: id);
            if (stats == null) { throw new HttpException((int)HttpStatusCode.NotFound, "PoolName was Not found"); }

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
        public async Task<ActionResult> Create([Bind(Include = "PoolName,ActiveDirectoryUserGroup,Logo,ActiveDirectoryAdminGroup,EmailNotifyList,RdpTcpPort,CleanupInMinutes,RemoteAdminUser,RemoteAdminPassword,WelcomeMessage")] Pool pool)
        {

            if (ModelState.IsValid)
            {
                pool.InitializationVector = this.Pw.NewInitializationVector();
                pool.RemoteAdminPassword =  this.Pw.Encrypt(pool.RemoteAdminPassword, pool.InitializationVector);
                await this.Svc.AddPoolAsync(pool);
                return RedirectToAction("Index");
            }

            return View(pool);
        }

        // GET: Pools/Edit/5
        [HttpGet]
        [PoolAdministratorAuthorize]
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null) { throw new HttpException((int)HttpStatusCode.BadRequest, "Missing PoolName"); }
            Pool pool = await this.Svc.GetPoolByIdAsync(PoolName:id);
            if (pool == null) { throw new HttpException((int)HttpStatusCode.NotFound, "PoolName was Not found"); }

            pool.RemoteAdminPassword = this.Pw.Decrypt(pool.RemoteAdminPassword, pool.InitializationVector);
            TempData["id"] = id;
            TempData["vector"] = pool.InitializationVector;
            return View(pool);
        }

        // POST: Pools/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PoolAdministratorAuthorize]
        public async Task<ActionResult> Edit([Bind(Include = "PoolName,ActiveDirectoryUserGroup,Logo,ActiveDirectoryAdminGroup,EmailNotifyList,RdpTcpPort,CleanupInMinutes,RemoteAdminUser,RemoteAdminPassword,WelcomeMessage")] Pool pool)
        {
            if (ModelState.IsValid)
            {
                string vector = (String)TempData["vector"];
                pool.RemoteAdminPassword = this.Pw.Encrypt(pool.RemoteAdminPassword,vector);
                await this.Svc.UpdatePoolAsync(pool);
                return RedirectToAction("Index");
            }
            return View(pool);
        }

        // GET: Pools/Delete/5
        [AdministratorAuthorize]
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null) { throw new HttpException((int)HttpStatusCode.BadRequest, "Missing PoolName"); }
            Pool pool = await this.Svc.GetPoolByIdAsync(PoolName:id);
            if (pool == null) { throw new HttpException((int)HttpStatusCode.NotFound, "PoolName was Not found"); }
            TempData["id"] = id;
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
