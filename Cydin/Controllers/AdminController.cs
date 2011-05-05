using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cydin.Models;
using Cydin.Builder;
using Cydin.Properties;
using System.Text;
using System.IO;

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
			DownloadStats stats = CurrentUserModel.Stats.GetTotalDownloadStats (pd, start, end);
			return Content (stats.ToJson ());
		}
		
		public ActionResult GetRepoDownloadStatsAsync (string period, string arg)
		{
			CurrentUserModel.CheckIsAdmin ();
			DateTime end;
			DateTime start;
			TimePeriod pd = TimePeriod.Auto;
			DownloadStats.ParseQuery (period, arg, out pd, out start, out end);
			DownloadStats stats = CurrentUserModel.Stats.GetTotalRepoDownloadStats (pd, start, end);
			return Content (stats.ToJson ());
		}
		
		public ActionResult GetTopDownloads (string period, string arg)
		{
			CurrentUserModel.CheckIsAdmin ();
			DateTime end;
			DateTime start;
			TimePeriod pd = TimePeriod.Auto;
			DownloadStats.ParseQuery (period, arg, out pd, out start, out end);
			
			List<DownloadInfo> stats = CurrentUserModel.Stats.GetTopDownloads (start, end);
			StringBuilder sb = new StringBuilder ();
			foreach (var di in stats) {
				if (sb.Length > 0)
					sb.Append (',');
				sb.AppendFormat ("{{\"count\":{0},\"platform\":\"{1}\",\"projectId\":{2},\"appVersion\":\"{3}\",\"name\":\"{4}\"}}", di.Count, di.Platform, di.Release.ProjectId, di.Release.TargetAppVersion, di.Release.AddinName + " v" + di.Release.Version);
			}
			return Content ("[" + sb + "]");
		}
		
		public ActionResult GetRepoDownloadsCSV (string period, string arg)
		{
			CurrentUserModel.CheckIsAdmin ();
			MemoryStream ms = new MemoryStream ();
			StreamWriter sw = new StreamWriter (ms);
			sw.Write (CurrentUserModel.Stats.GetRepoDownloadStatsCSV ());
			sw.Flush ();
			ms.Position = 0;
			return File (ms, "text/plain", "repository-download.csv");
		}
		
		public ActionResult GetDownloadsCSV (string period, string arg)
		{
			CurrentUserModel.CheckIsAdmin ();
			MemoryStream ms = new MemoryStream ();
			StreamWriter sw = new StreamWriter (ms);
			sw.Write (CurrentUserModel.Stats.GetDownloadStatsCSV ());
			sw.Flush ();
			ms.Position = 0;
			return File (ms, "text/plain", "addins-download.csv");
		}
    }
}
