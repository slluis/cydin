using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cydin.Models;

namespace Cydin.Controllers
{
    public class AppReleaseController : Controller
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
			UserModel m = UserModel.GetCurrent ();
			ViewData["Creating"] = false;
			return View (m.GetAppRelease (id));
        }


        //
        // GET: /AppRelease/Delete/5
 
        public ActionResult Delete(int id)
        {
            return View();
        }


		[HttpPost]
		public ActionResult UploadAssemblies (AppRelease release)
		{
			UserModel m = UserModel.GetCurrent ();
			AppRelease rp = m.GetAppRelease (release.Id);
			rp.AppVersion = release.AppVersion;
			rp.AddinRootVersion = release.AddinRootVersion;
			m.UpdateAppRelease (rp, Request.Files.Count > 0 ? Request.Files[0] : null);
			return RedirectToAction ("Index", "Admin");
		}
	}
}
