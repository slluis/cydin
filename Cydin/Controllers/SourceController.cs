using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cydin.Models;
using Cydin.Builder;

namespace Cydin.Controllers
{
    public class SourceController : CydinController
    {
        //
        // GET: /Source/

        public ActionResult Index(int projectId)
        {
			ViewData["ProjectId"] = projectId;
			return View (CurrentUserModel.GetSources (projectId));
        }

        //
        // GET: /Source/Create

		public ActionResult Create (int projectId)
        {
			ViewData["Creating"] = true;
			ViewData["CreatingProject"] = false;
			VcsSource s = new VcsSource ();
			s.ProjectId = projectId;
            return View("Edit", s);
        }

		public ActionResult CreateInitial (int projectId)
		{
			ViewData["Creating"] = true;
			ViewData["CreatingProject"] = true;
			VcsSource s = new VcsSource ();
			s.ProjectId = projectId;
			return View ("Edit", s);
		}

		//
        // POST: /Source/Create

        [HttpPost]
        public ActionResult Create(VcsSource source, bool? initialCreation)
        {
			CurrentUserModel.CreateSource (source);
			if (initialCreation.HasValue && initialCreation.Value)
				return RedirectToAction ("Index", "Project", new { id = source.ProjectId });
			else
				return RedirectToAction ("Index", new { projectId = source.ProjectId });
        }
        
        //
        // GET: /Source/Edit/5
 
        public ActionResult Edit(int id)
        {
			ViewData["Creating"] = false;
			ViewData["CreatingProject"] = false;
			return View (CurrentUserModel.GetSource (id));
        }

        //
        // POST: /Source/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, VcsSource source)
        {
            try
            {
				CurrentUserModel.UpdateSource (source);
				return RedirectToAction ("Index", new { projectId = source.ProjectId });
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Source/Delete/5
 
        public ActionResult Delete(int id, int projectId)
        {
			CurrentUserModel.DeleteSource (id);
			return RedirectToAction ("Index", new { projectId = projectId });
        }
		
		public ActionResult AddinProjectHelp (int projectId)
		{
            return View("AddinProjectHelp");
		}

    }
}
