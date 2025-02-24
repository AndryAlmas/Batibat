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
            "~/Scripts/jquery-3.5.1.slim.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
            "~/Scripts/bootstrap-4.5.2.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/popper").Include(
            "~/Scripts/popper-2.5.2.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/fontawesome").Include(
            "~/Scripts/FontAwesome-6.7.1.js"));

            bundles.Add(new ScriptBundle("~/bundles/glide").Include(
            "~/Scripts/glide.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/custom").Include(
            "~/Scripts/custom.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Styles/bootstrap-4.5.2.min.css",
                      "~/Styles/glide.core.min.css",
                      "~/Styles/glide.theme.min.css",
                      "~/Styles/site.css"));
        }
    }
}
