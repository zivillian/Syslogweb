using System.Web;
using System.Web.Optimization;

namespace SyslogWeb
{
	public class BundleConfig
	{
		// Weitere Informationen zu Bundling finden Sie unter "http://go.microsoft.com/fwlink/?LinkId=254725".
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
						"~/Scripts/jquery-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
						"~/Scripts/jquery-ui-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
						"~/Scripts/jquery.unobtrusive*",
						"~/Scripts/jquery.validate*"));
			
			bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
				"~/Scripts/bootstrap.js",
				"~/Scripts/moment-with-locales.js",
				"~/Scripts/bootstrap-datetimepicker.js"));
			
			bundles.Add(new ScriptBundle("~/bundles/syslogweb").Include(
				"~/Scripts/mustache.js",
				"~/Scripts/Prettify/prettify.js",
				"~/Scripts/SyslogWeb.js")); 

			bundles.Add(new StyleBundle("~/Content/css").Include(
				"~/Content/Site.css",
				"~/Content/bootstrap.css",
				"~/Content/bootstrap-theme.css",
				"~/Content/bootstrap-datetimepicker.css",
				"~/Content/Prettify/prettify.css"));

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

		}
	}
}