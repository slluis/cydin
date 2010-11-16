using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Cydin.Properties;
using Cydin.Controllers;
using Cydin.Models;

namespace Cydin
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterRoutesCommon (RouteCollection routes)
		{
			routes.IgnoreRoute ("{resource}.axd/{*pathInfo}");
			routes.Add (new Route ("addins/{platform}/{version}/{*file}", new FileHandler ("Stable")));
			routes.Add (new Route ("Stable/{platform}/{version}/{*file}", new FileHandler ("Stable")));
			routes.Add (new Route ("Beta/{platform}/{version}/{*file}", new FileHandler ("Beta")));
			routes.Add (new Route ("Alpha/{platform}/{version}/{*file}", new FileHandler ("Alpha")));
			routes.Add (new Route ("package/{upload}", new PackageUploadHandler ()));
			routes.Add (new Route ("service/{events}", new ServiceEventsHandler ()));
		}
		
		public static void RegisterRoutesSingleApp (RouteCollection routes)
		{
			RegisterRoutesCommon (routes);

			routes.MapRoute (
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);
		}
		
		public static void RegisterRoutesMultiAppDomain (RouteCollection routes)
		{
			RegisterRoutesSingleApp (routes);
		}
		
		public static void RegisterRoutesMultiAppPath (RouteCollection routes)
		{
			RegisterRoutesCommon (routes);

			routes.MapRoute (
				"Default", // Route name
				"{app}/{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);

			routes.MapRoute (
				"DefaultHome", // Route name
				"", // URL with parameters
				new { controller = "SiteHome", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);
		}
		
		public static void UpdateRoutes ()
		{
			foreach (var r in RouteTable.Routes.ToList ())
				RouteTable.Routes.Remove (r);
			
			switch (Settings.Default.OperationMode) {
			case OperationMode.NotSet: RegisterRoutesSingleApp (RouteTable.Routes); break;
			case OperationMode.SingleApp: RegisterRoutesSingleApp (RouteTable.Routes); break;
			case OperationMode.MultiAppDomain: RegisterRoutesMultiAppDomain (RouteTable.Routes); break;
			case OperationMode.MultiAppPath: RegisterRoutesMultiAppPath (RouteTable.Routes); break;
			}
		}

		protected void Application_Start ()
		{
			System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate {
				return true;
			};
			
			AreaRegistration.RegisterAllAreas ();
			UpdateRoutes ();
			Settings.BasePath = Server.MapPath ("/");
		}
	}
	
	class AppConstraint: IRouteConstraint
	{
		public bool Match (HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			return (string) values ["controller"] != "md";
		}
	}
}