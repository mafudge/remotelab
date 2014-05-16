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
    public class PoolUserAuthorize : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var Svc = ((RemoteLab.Controllers.PoolsController)filterContext.Controller).Svc;
            String PoolName = filterContext.ActionParameters["Id"].ToString();
            Pool pool = Svc.GetPoolById(PoolName);

            var valid = (((ClaimsPrincipal)filterContext.HttpContext.User).Claims.Any(c =>
                c.Value.Equals(pool.ActiveDirectoryUserGroup, StringComparison.InvariantCultureIgnoreCase) &&
                c.Type == ClaimTypes.Role
                ));

            if (!valid)  //TODO: Redirect or show unauthorize page??
            {
                filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                filterContext.HttpContext.Response.End();
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}