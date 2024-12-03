using System.Web;
using System.Web.Optimization;

namespace Batibatlocation
{
    public class BundleConfig
    {
        // Per altre informazioni sulla creazione di bundle, vedere https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
            "~/Scripts/jquery-{version}.slim.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
            "~/Scripts/bootstrap-{version}.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/popper").Include(
            "~/Scripts/popper-{version}.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/fontawesome").Include(
            "~/Scripts/FontAwesome.js"));

            bundles.Add(new ScriptBundle("~/bundles/custom").Include(
            "~/Scripts/custom.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Styles/bootstrap-{version}.min.css",
                      "~/Styles/site.css"));
        }
    }
}
