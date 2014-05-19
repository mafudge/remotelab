using System.Web;
using System.Web.Optimization;

namespace RemoteLab
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.validate.js",
                        "~/Scripts/jquery.validate.unobtrusive.js",
                        "~/Scripts/jquery-fancybox.js"));

            bundles.Add(new ScriptBundle("~/bundles/browserdetect").Include(
                        "~/Scripts/BrowserDetect.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                     "~/Content/bootstrap.css",
                    "~/Content/bootstrap-theme.css",
                    "~/Content/font-awesome.css",
                    "~/Content/jquery.fancybox.css"));
        }
    }
}
