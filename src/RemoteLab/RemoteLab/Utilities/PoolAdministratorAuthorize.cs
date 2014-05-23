using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using RemoteLab.Models;
using System.Security.Claims;
using System.Web.Http;
using RemoteLab.Utilities;


namespace RemoteLab.Utilities
{

    /// <summary>
    /// Authorization attribute check for user an administrator of the pool 
    /// </summary>
    public class PoolAdministratorAuthorize : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            var Svc = ((RemoteLab.Controllers.PoolsController)filterContext.Controller).Svc;
            var User = ((ClaimsPrincipal)filterContext.HttpContext.User);
            String PoolName = String.Empty;
            bool valid = false;
            try
            {
                PoolName = filterContext.RequestContext.HttpContext.Request.RequestType == "GET"
                                ? filterContext.ActionParameters["id"].ToString()
                                : filterContext.Controller.TempData["id"].ToString();
                Pool pool = Svc.GetPoolById(PoolName);
                valid = (pool != null) && User.IsAdministratorOfPool(pool);
                            
            }
            catch (NullReferenceException)
            {
                valid = false;                
            }

            // check the Admin Claim
            valid = valid || User.IsAdministrator();

            if (!valid)  // Forbidden
            {
                throw new HttpException((int)System.Net.HttpStatusCode.Forbidden,"Not permitted to access this resource");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}