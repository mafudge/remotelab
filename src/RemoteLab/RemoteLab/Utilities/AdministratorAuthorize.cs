using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;

namespace RemoteLab.Utilities
{
    /// <summary>
    /// Authorization attribute - to check if user is an administrator
    /// </summary>
    public class AdministratorAuthorize : AuthorizeAttribute 
    {
        protected override Boolean AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException("httpContext");

            if (!httpContext.User.Identity.IsAuthenticated) return false;

            // return whether based on user having admin claim
            return ((ClaimsPrincipal)httpContext.User).IsAdministrator();

        }
    }
}