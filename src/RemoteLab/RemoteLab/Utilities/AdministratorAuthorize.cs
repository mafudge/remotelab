using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;

namespace RemoteLab.Utilities
{
    public class AdministratorAuthorize : AuthorizeAttribute 
    {
        protected override Boolean AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException("httpContext");

            if (!httpContext.User.Identity.IsAuthenticated) return false;

            // return whether based on user having admin claim
            return (((ClaimsPrincipal)httpContext.User).Claims.Any(c => 
                c.Value.Equals(Properties.Settings.Default.AdministratorADGroup, StringComparison.InvariantCultureIgnoreCase) &&
                c.Type == ClaimTypes.Role
                ));

        }
    }
}