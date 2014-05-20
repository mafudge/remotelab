using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using RemoteLab.Models;
using System.Security.Claims;
using System.Web.Http;


namespace RemoteLab.Utilities
{
    public class PoolAdministratorAuthorize : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            var Svc = ((RemoteLab.Controllers.PoolsController)filterContext.Controller).Svc;
            String PoolName = String.Empty;
            bool valid = false;
            try
            {
                PoolName = filterContext.RequestContext.HttpContext.Request.RequestType == "GET"
                                ? filterContext.ActionParameters["id"].ToString()
                                : filterContext.Controller.TempData["id"].ToString();
                Pool pool = Svc.GetPoolById(PoolName);
                valid = (pool != null) &&
                            (((ClaimsPrincipal)filterContext.HttpContext.User).Claims.Any(c =>
                                c.Value.Equals(pool.ActiveDirectoryAdminGroup, StringComparison.InvariantCultureIgnoreCase) &&
                                c.Type == ClaimTypes.Role
                            ));
            }
            catch (NullReferenceException)
            {
                valid = false;                
            }

            // check the Admin Claim
            valid = valid || (((ClaimsPrincipal)filterContext.HttpContext.User).Claims.Any(c =>
                c.Value.Equals(Properties.Settings.Default.AdministratorADGroup, StringComparison.InvariantCultureIgnoreCase) &&
                c.Type == ClaimTypes.Role
                ));


            if (!valid)  //TODO: Redirect or show unauthorize page??
            {
                filterContext.HttpContext.Response.StatusCode= (int)System.Net.HttpStatusCode.Unauthorized;
                filterContext.HttpContext.Response.End();
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}