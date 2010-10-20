using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cydin.Models;
using Cydin.Builder;

namespace Cydin.Controllers
{
    public class ReviewController : CydinController
    {
        //
        // GET: /Review/

        public ActionResult Index()
        {
			CurrentUserModel.CheckIsAdmin ();
            return View();
        }

		public ActionResult ApproveRelease (int id)
		{
			CurrentUserModel.CheckIsAdmin ();
			CurrentUserModel.ApproveRelease (id);
			BuildService.UpdateRepositories (false);
			return View ("Index");
		}

		public ActionResult RejectRelease (int id)
		{
			CurrentUserModel.CheckIsAdmin ();
			CurrentUserModel.RejectRelease (id);
			BuildService.UpdateRepositories (false);
			return View ("Index");
		}
	}
}
