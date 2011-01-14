using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cydin.Models;

namespace Cydin.Controllers
{
    public class AppReleaseController : CydinController
    {
        //
        // GET: /AppRelease/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /AppRelease/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /AppRelease/Create

        public ActionResult Create()
        {
			AppRelease rel = new AppRelease ();
			ViewData["Creating"] = true;
            return View("Edit", rel);
        } 

        //
        // GET: /AppRelease/Edit/5
 
        public ActionResult Edit(int id)
        {
			ViewData["Creating"] = false;
			return View (CurrentUserModel.GetAppRelease (id));
        }


        //
        // GET: /AppRelease/Delete/5
 
        public ActionResult Delete(int id)
        {
			CurrentUserModel.DeleteAppRelease (id);
			return RedirectToAction ("Index", "Admin");
        }

		[HttpPost]
		public ActionResult UpdateRelease (AppRelease release)
		{
			AppRelease rp = CurrentUserModel.GetAppRelease (release.Id);
			rp.AppVersion = release.AppVersion;
			rp.AddinRootVersion = release.AddinRootVersion;
			rp.CompatibleAppReleaseId = release.CompatibleAppReleaseId;
			CurrentUserModel.UpdateAppRelease (rp, Request.Files.Count > 0 ? Request.Files[0] : null);
			return RedirectToAction ("Index", "Admin");
		}

		[HttpPost]
		public ActionResult CreateRelease (AppRelease release)
		{
			CurrentUserModel.CreateAppRelease (release, Request.Files.Count > 0 ? Request.Files[0] : null);
			return RedirectToAction ("Index", "Admin");
		}
	}
}
