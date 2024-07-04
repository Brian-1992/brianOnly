using System.Web.Optimization;

namespace WebApp
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //index.cshtml
            bundles.Add(new ScriptBundle("~/bundles/extjs").Include(
                    "~/Scripts/jquery-1.10.2.min.js",
                     "~/Scripts/extjs6/ext-all.js",
                     "~/Scripts/extjs6/ext-locale-zh_TW.js",
                     "~/Scripts/default.js",
                     "~/Scripts/TRAUtility.js"));

            //_Layout.cshtml
            bundles.Add(new ScriptBundle("~/bundles/extjs_Layout").Include(
                    "~/Scripts/jquery-1.10.2.min.js",
                     "~/Scripts/extjs6/ext-all.js",
                     "~/Scripts/extjs6/ext-locale-zh_TW.js",
                     "~/Scripts/TRAUtility.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // 使用開發版本的 Modernizr 進行開發並學習。然後，當您
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            //Scripts for RWD
            bundles.Add(new ScriptBundle("~/bundles/rwd").Include(
                      "~/Scripts/jquery-3.7.1.min.js",
                      //"~/Scripts/jquery-ui-1.8.24.min.js",
                      "~/Scripts/popper/umd/popper.js",
                      "~/Scripts/popper/umd/popper-utils.js",
                      "~/Scripts/bootstrap-4.6.2-dist/js/bootstrap.min.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css",
                      "~/Content/fontawesome/css/all.min.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));

            //BundleTable.EnableOptimizations = true;
        }
    }
}
