using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cydin.Models;
using Cydin.Builder;
using Cydin.Properties;

namespace Cydin.Controllers
{
    public class AdminController : CydinController
    {
        public ActionResult Index()
        {
			CurrentUserModel.CheckIsAdmin ();
			return View ();
        }

		public ActionResult UpdateRepositories ()
		{
			CurrentUserModel.CheckIsAdmin ();
			BuildService.UpdateRepositories (true);
			return View ("Index");
		}
		
		public ActionResult ProjectsList ()
		{
			CurrentUserModel.CheckIsAdmin ();
			return View ();
		}
		
		public ActionResult AddAdminAsync (string email)
		{
			CurrentUserModel.CheckIsAdmin ();
			User u = CurrentServiceModel.GetUserByEmail (email);
			if (u != null) {
				CurrentUserModel.SetUserApplicationPermission (u.Id, ApplicationPermission.Administer, true);
				return Content ("OK");
			}
			else
				return Content ("");
		}
		
		public ActionResult RemoveAdmin (int userId)
		{
			CurrentUserModel.CheckIsAdmin ();
			CurrentUserModel.SetUserApplicationPermission (userId, ApplicationPermission.Administer, false);
			return RedirectToAction ("Index");
		}
		
		public ActionResult GetDownloadStatsAsync (string period, string arg)
		{
			CurrentUserModel.CheckIsAdmin ();
			DateTime end;
			DateTime start;
			TimePeriod pd = TimePeriod.Auto;
			DownloadStats.ParseQuery (period, arg, out pd, out start, out end);
			DownloadStats stats = CurrentUserModel.GetTotalDownloadStats (pd, start, end);
			return Content (stats.ToJson ());
		}
		
		public ActionResult GetRepoDownloadStatsAsync (string period, string arg)
		{
			CurrentUserModel.CheckIsAdmin ();
			DateTime end;
			DateTime start;
			TimePeriod pd = TimePeriod.Auto;
			DownloadStats.ParseQuery (period, arg, out pd, out start, out end);
			DownloadStats stats = CurrentUserModel.GetTotalRepoDownloadStats (pd, start, end);
			return Content (stats.ToJson ());
		}
    }
}
