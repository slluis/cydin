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
    public class ProjectController : Controller
    {
        //
        // GET: /Project/

        public ActionResult Index (int id)
        {
			UserModel m = UserModel.GetCurrent ();
			Project p = m.GetProject (id);
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
			UserModel m = UserModel.GetCurrent ();
			m.ValidateProject (id);
			ViewData["Creating"] = false;
			return View ("Create", m.GetProject (id));
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
				UserModel m = UserModel.GetCurrent ();
				m.CreateProject (project);
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
				UserModel m = UserModel.GetCurrent ();
				Project p = m.GetProject (project.Id);
				p.Name = project.Name;
				p.Description = project.Description;
				m.UpdateProject (p);
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
			UserModel.GetCurrent ().DeleteProject (id);
			return RedirectToAction ("Index", "Home");
        }

		public ActionResult DeleteRelease (int releaseId)
		{
			UserModel m = UserModel.GetCurrent ();
			Release r = m.GetRelease (releaseId);
			m.DeleteRelease (r);
			return RedirectToAction ("Index", new { id = r.ProjectId });
		}

		public ActionResult PublishRelease (int sourceId)
		{
			UserModel m = UserModel.GetCurrent ();
			Release rel = m.PublishRelease (sourceId);
			return RedirectToAction ("Index", new { id = rel.ProjectId});
		}

		public ActionResult UploadRelease (int projectId)
		{
			UserModel m = UserModel.GetCurrent ();
			Project p = m.GetProject (projectId);
			return View (p);
		}

		[HttpPost]
		public ActionResult UploadReleaseFile (int projectId)
		{
			UserModel m = UserModel.GetCurrent ();
			m.UploadRelease (projectId, Request.Files[0]);
			return RedirectToAction ("Index", new { id = projectId });
		}

		public ActionResult UpdateSource (int sourceTagId)
		{
			UserModel m = UserModel.GetCurrent ();
			m.CleanSources (sourceTagId);
			BuildService.TriggerBuild ();
			SourceTag st = m.GetSourceTag (sourceTagId);
			return RedirectToAction ("Index", new { id = st.ProjectId });
		}

		public ActionResult BuildLog (int id)
		{
			UserModel m = UserModel.GetCurrent ();
			SourceTag stag = m.GetSourceTag (id);
			return File (stag.LogFile, "text/html");
		}

		public ActionResult ReleasePackage (int id, string platform)
		{
			UserModel m = UserModel.GetCurrent ();
			Release rel = m.GetRelease (id);
			return File (rel.GetFilePath (platform), "application/x-mpack", rel.AddinId + "-" + rel.Version + ".mpack");
		}

		public ActionResult SourceTagPackage (int id, string platform)
		{
			UserModel m = UserModel.GetCurrent ();
			SourceTag stag = m.GetSourceTag (id);
			return File (stag.GetFilePath (platform), "application/x-mpack", stag.AddinId + "-" + stag.AddinVersion + ".mpack");
		}

		public ActionResult AppReleasePackage (int id)
		{
			ServiceModel m = ServiceModel.GetCurrent ();
			AppRelease release = m.GetAppRelease (id);
			return File (release.ZipPath, "application/zip");
		}

		public ActionResult ReleasePackageInstaller (int id)
		{
			UserModel m = UserModel.GetCurrent ();
			Release rel = m.GetRelease (id);
			StringWriter sw = new StringWriter ();
			BuildService.GenerateInstallerXml (sw, m, rel, rel.PlatformsList);
			return File (Encoding.UTF8.GetBytes (sw.ToString()), "application/x-" + m.CurrentApplication.AddinPackageSubextension + "-mpack", rel.AddinId + "-" + rel.Version + m.CurrentApplication.AddinPackageExtension);
		}

		public ActionResult ConfirmDelete (int id)
		{
			UserModel m = UserModel.GetCurrent ();
			Project p = m.GetProject (id);
			return View (p);
		}

		public ActionResult ToggleTrusted (int id)
		{
			UserModel m = UserModel.GetCurrent ();
			if (!m.IsAdmin)
				throw new Exception ("Unauthorised");
			Project p = m.GetProject (id);
			m.SetProjectTrusted (id, !p.Trusted);
			return View ("Index", p);
		}
		
        [HttpPost]
		public ActionResult SetNotification (int id, string notif, string value)
		{
			UserModel m = UserModel.GetCurrent ();
			
			if (!notif.StartsWith ("notify-"))
				return Content ("Unknown notification");
			ProjectNotification pnot = (ProjectNotification) Enum.Parse (typeof(ProjectNotification), notif.Substring (7));
			m.SetProjectNotification (pnot, id, value == "true");
			return Content ("OK");
		}
		
        [HttpPost]
		public ActionResult SetDevStatus (int id, int stagId, int value)
		{
			UserModel m = UserModel.GetCurrent ();
			m.ValidateProject (id);
			
			SourceTag stag = m.GetSourceTag (stagId);
			if (stag.ProjectId != id)
				throw new InvalidOperationException ("Invalid source tag");
			
			stag.DevStatus = (DevStatus) value;
			m.UpdateSourceTag (stag);
			return Content ("OK");
		}
	}
}
