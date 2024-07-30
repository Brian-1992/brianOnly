using System.Web;
using System.Web.Optimization;

namespace WebAppVen
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //index.cshtml
            bundles.Add(new ScriptBundle("~/bundles/extjs").Include(
                    "~/Scripts/jquery-3.1.1.min.js",
                     "~/Scripts/extjs6/ext-all.js",
                     "~/Scripts/extjs6/ext-locale-zh_TW.js",
                     "~/Scripts/default.js",
                     "~/Scripts/TRAUtility.js"));

            //_Layout.cshtml
            bundles.Add(new ScriptBundle("~/bundles/extjs_Layout").Include(
                    "~/Scripts/jquery-3.1.1.min.js",
                     "~/Scripts/extjs6/ext-all.js",
                     "~/Scripts/extjs6/ext-locale-zh_TW.js",
                     "~/Scripts/TRAUtility.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-3.1.1.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css",
                      "~/Content/fontawesome/css/all.min.css"));

            //BundleTable.EnableOptimizations = true;
        }
    }
}
