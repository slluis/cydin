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
	public class HomeController : CydinController
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
			else if (CurrentUserModel.CurrentApplication == null) {
				if (CurrentUserModel.User == null)
					return Redirect (ControllerHelper.GetActionUrl ("home", "Login", "User"));
				else
					return RedirectToAction ("Index", "SiteAdmin");
			}
				
			return View ();
		}

		public ActionResult About ()
		{
			return View ();
		}
		
        [HttpPost]
		public ActionResult SetApplicationNotification (string notif, string value)
		{
			UserModel m = CurrentUserModel;
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
			CurrentUserModel.CheckIsAdmin ();
			return View ("Edit");
		}
 
        [HttpPost]
		[ValidateInput(false)]
        public ActionResult Update (string content)
        {
            try
            {
				UserModel m = CurrentUserModel;
				m.CheckIsAdmin ();
				Application app = CurrentServiceModel.GetApplication (m.CurrentApplication.Id);
				app.Description = content;
				CurrentServiceModel.UpdateApplication (app);
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
