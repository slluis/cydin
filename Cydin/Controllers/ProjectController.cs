using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Cydin.Models;
using Cydin.Builder;

namespace Cydin.Controllers
{
    public class ProjectController : CydinController
    {
        //
        // GET: /Project/

        public ActionResult Index (int id)
        {
			Project p = CurrentUserModel.GetProject (id);
			if (p == null)
				throw new Exception ("Project not found");
			return View (p);
        }

		//
        // GET: /Project/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }
		
		public ActionResult Edit (int id)
		{
			CurrentUserModel.ValidateProject (id);
			ViewData["Creating"] = false;
			return View ("Create", CurrentUserModel.GetProject (id));
		}

        public ActionResult Create()
        {
			ViewData["Creating"] = true;
            return View();
        } 

        [HttpPost]
        public ActionResult Create(Project project)
        {
            try
            {
				CurrentUserModel.CreateProject (project);
				return RedirectToAction ("CreateInitial", "Source", new { projectId = project.Id });
            }
            catch (Exception ex)
            {
                return Content (ex.ToString ());
            }
        }
 
        [HttpPost]
        public ActionResult Edit (Project project)
        {
            try
            {
				Project p = CurrentUserModel.GetProject (project.Id);
				p.Name = project.Name;
				p.Description = project.Description;
				CurrentUserModel.UpdateProject (p);
				return RedirectToAction ("Index", new { id = project.Id });
            }
            catch (Exception ex)
            {
                return Content (ex.ToString ());
            }
        }

        //
        // GET: /Project/Delete/5
 
        public ActionResult Delete(int id)
        {
			CurrentUserModel.DeleteProject (id);
			return RedirectToAction ("Index", "Home");
        }

		public ActionResult DeleteRelease (int releaseId)
		{
			Release r = CurrentUserModel.GetRelease (releaseId);
			CurrentUserModel.DeleteRelease (r);
			return RedirectToAction ("Index", new { id = r.ProjectId });
		}

		public ActionResult PublishRelease (int sourceId)
		{
			Release rel = CurrentUserModel.PublishRelease (sourceId);
			return RedirectToAction ("Index", new { id = rel.ProjectId});
		}

		public ActionResult UploadRelease (int projectId)
		{
			Project p = CurrentUserModel.GetProject (projectId);
			return View (p);
		}

		[HttpPost]
		public ActionResult UploadReleaseFile (int projectId)
		{
			List<string> platforms = new List<string> ();
			foreach (string plat in CurrentUserModel.CurrentApplication.PlatformsList) {
				string val = Request.Form ["platform-" + plat];
				if (val.ToLower ().IndexOf ("true") != -1)
					platforms.Add (plat);
			}
			CurrentUserModel.UploadRelease (projectId, Request.Files[0], Request.Form ["appVersion"], platforms.ToArray ());
			return RedirectToAction ("Index", new { id = projectId });
		}
		
		public ActionResult DeleteUpload (int sourceId)
		{
			SourceTag st = CurrentUserModel.GetSourceTag (sourceId);
			CurrentUserModel.DeleteSourceTag (st);
			return RedirectToAction ("Index", new { id = st.ProjectId });
		}

		public ActionResult UpdateSource (int sourceTagId)
		{
			CurrentUserModel.CleanSources (sourceTagId);
			SourceTag st = CurrentUserModel.GetSourceTag (sourceTagId);
			BuildService.Build (CurrentUserModel.CurrentApplication.Id, st.ProjectId);
			return RedirectToAction ("Index", new { id = st.ProjectId });
		}

		public ActionResult BuildLog (int id)
		{
			SourceTag stag = CurrentUserModel.GetSourceTag (id);
			return File (stag.LogFile, "text/html");
		}

		public ActionResult ReleasePackage (int id, string platform)
		{
			Release rel = CurrentUserModel.GetRelease (id);
			return File (rel.GetFilePath (platform), "application/x-mpack", rel.AddinId + "-" + rel.Version + ".mpack");
		}

		public ActionResult SourceTagPackage (int id, string platform)
		{
			SourceTag stag = CurrentUserModel.GetSourceTag (id);
			return File (stag.GetFilePath (platform), "application/x-mpack", stag.AddinId + "-" + stag.AddinVersion + ".mpack");
		}

		public ActionResult AppReleasePackage (int id)
		{
			AppRelease release = CurrentServiceModel.GetAppRelease (id);
			return File (release.ZipPath, "application/zip", Path.GetFileName (release.ZipPath));
		}

		public ActionResult ReleasePackageInstaller (int id)
		{
			Release rel = CurrentUserModel.GetRelease (id);
			StringWriter sw = new StringWriter ();
			BuildService.GenerateInstallerXml (sw, CurrentUserModel, rel, rel.PlatformsList);
			return File (Encoding.UTF8.GetBytes (sw.ToString()), "application/x-" + CurrentUserModel.CurrentApplication.AddinPackageSubextension + "-mpack", rel.AddinId + "-" + rel.Version + CurrentUserModel.CurrentApplication.AddinPackageExtension);
		}

		public ActionResult ConfirmDelete (int id)
		{
			Project p = CurrentUserModel.GetProject (id);
			return View (p);
		}

		public ActionResult UpdateFlags (int projectId, bool allowDirectPublish, bool allowPackageUpload)
		{
			if (!CurrentUserModel.IsAdmin)
				throw new Exception ("Unauthorised");
			
			ProjectFlag flags = ProjectFlag.None;
			if (allowDirectPublish)
				flags |= ProjectFlag.AllowDirectPublish;
			if (allowPackageUpload)
				flags |= ProjectFlag.AllowPackageUpload;
			CurrentUserModel.UpdateProjectFlags (projectId, flags);
			return RedirectToAction ("Index", new { id=projectId });
		}
		
        [HttpPost]
		public ActionResult SetNotification (int id, string notif, string value)
		{
			if (!notif.StartsWith ("notify-"))
				return Content ("Unknown notification");
			ProjectNotification pnot = (ProjectNotification) Enum.Parse (typeof(ProjectNotification), notif.Substring (7));
			CurrentUserModel.SetProjectNotification (pnot, id, value == "true");
			return Content ("OK");
		}
		
        [HttpPost]
		public ActionResult SetDevStatus (int id, int stagId, int value)
		{
			CurrentUserModel.ValidateProject (id);
			
			SourceTag stag = CurrentUserModel.GetSourceTag (stagId);
			if (stag.ProjectId != id)
				throw new InvalidOperationException ("Invalid source tag");
			
			stag.DevStatus = (DevStatus) value;
			CurrentUserModel.UpdateSourceTag (stag);
			return Content ("OK");
		}
		
		public ActionResult AddOwnerAsync (int id, string email)
		{
			CurrentUserModel.ValidateProject (id);
			User u = CurrentServiceModel.GetUserByEmail (email);
			if (u != null) {
				CurrentUserModel.AddProjectOwner (id, u.Id);
				return Content ("OK");
			}
			else
				return Content ("");
		}
		
		public ActionResult RemoveOwner (int id, int userId)
		{
			CurrentUserModel.ValidateProject (id);
			CurrentUserModel.RemoveProjectOwner (id, userId);
			return RedirectToAction ("Index", new { id=id });
		}
		
		public ActionResult Stats (int projectId, int releaseId)
		{
			ViewData["projectId"] = projectId;
			ViewData["releaseId"] = releaseId;
			return View ();
		}
		
		public ActionResult GetStatsAsync (int pid, int relid, string period, string arg)
		{
			DateTime end;
			DateTime start;
			TimePeriod pd = TimePeriod.Auto;
			DownloadStats.ParseQuery (period, arg, out pd, out start, out end);
			DownloadStats stats = relid != -1 ? CurrentUserModel.Stats.GetReleaseDownloadStats (relid, pd, start, end) : CurrentUserModel.Stats.GetProjectDownloadStats (pid, pd, start, end);
			return Content (stats.ToJson ());
		}
	}
}
