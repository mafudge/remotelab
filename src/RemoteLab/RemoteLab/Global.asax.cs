using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace RemoteLab
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Name;
            StructuremapMvc.Start();
            ApplicationInit();
        }

        private void ApplicationInit()
        {
            var Assembly = typeof(MvcApplication).Assembly.GetName();
            var versionInfo = FileVersionInfo.GetVersionInfo(typeof(MvcApplication).Assembly.Location);
            Version version = Assembly.Version;
            var App = HttpContext.Current.Application;
            App.Lock();
            App.Add("VERSION", String.Format("{0}.{1}", version.Major, version.Minor));
            App.Add("BUILD", String.Format("{0}.{1}", version.Build, version.Revision));
            App.Add("COPYRIGHT", versionInfo.LegalCopyright);
            App.Add("NAME",versionInfo.ProductName);
            App.UnLock();
            
        }
    }
}
