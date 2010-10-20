using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cydin.Properties;
using Cydin.Models;

namespace Cydin.Controllers
{
	[HandleError]
	public class HomeController : Controller
	{
		public ActionResult Index ()
		{
			if (Settings.Default.InitialConfiguration)
				return RedirectToAction ("Setup", "SiteAdmin");
			
			string host = HttpContext.Request.Url.Host;
			if (HttpContext.Request.Url.Port != 80)
				host += ":" + HttpContext.Request.Url.Port;
			
			if (host != Settings.Default.WebSiteHost)
				return Redirect (ControllerHelper.GetActionUrl ("home", null, null));
			
			if (Settings.Default.SupportsMultiApps) {
				string app = UserModel.GetCurrentAppName ();
				if (string.IsNullOrEmpty (app) || app == "home")
					return Redirect (ControllerHelper.GetActionUrl ("home", "Index", "SiteHome"));
			}
			else if (UserModel.GetCurrent ().CurrentApplication == null)
				return RedirectToAction ("Index", "SiteAdmin");
				
			return View ();
		}

		public ActionResult About ()
		{
			return View ();
		}
		
        [HttpPost]
		public ActionResult SetApplicationNotification (string notif, string value)
		{
			UserModel m = UserModel.GetCurrent ();
			NotificationInfo ni = m.GetApplicationNotifications ().FirstOrDefault (n => n.Id == notif);
			if (ni != null) {
				bool enable = value == "true";
				if (ni.Value is ProjectNotification)
					m.SetApplicationNotification ((ProjectNotification) ni.Value, enable);
				else if (ni.Value is ApplicationNotification)
					m.SetApplicationNotification ((ApplicationNotification) ni.Value, enable);
				else
					m.SetSiteNotification ((SiteNotification) ni.Value, enable);
			}
			else
				return Content ("Unknown notification");
			
			return Content ("OK");
		}
		
		public ActionResult Edit ()
		{
			UserModel m = UserModel.GetCurrent ();
			m.CheckIsAdmin ();
			return View ("Edit");
		}
 
        [HttpPost]
		[ValidateInput(false)]
        public ActionResult Update (string content)
        {
            try
            {
				UserModel m = UserModel.GetCurrent ();
				m.CheckIsAdmin ();
				ServiceModel sm = ServiceModel.GetCurrent ();
				Application app = sm.GetApplication (m.CurrentApplication.Id);
				app.Description = content;
				sm.UpdateApplication (app);
				Cydin.Views.ViewHelper.ClearCache ();
				
				return RedirectToAction ("Index");
            }
            catch (Exception ex)
            {
                return Content (ex.ToString ());
            }
        }
	}
}
