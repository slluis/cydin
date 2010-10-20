using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cydin.Models;
using Cydin.Builder;

namespace Cydin.Controllers
{
    public class ReviewController : Controller
    {
        //
        // GET: /Review/

        public ActionResult Index()
        {
			UserModel m = UserModel.GetCurrent ();
			m.CheckIsAdmin ();
            return View(UserModel.GetCurrent ());
        }

		public ActionResult ApproveRelease (int id)
		{
			UserModel m = UserModel.GetCurrent ();
			m.CheckIsAdmin ();
			m.ApproveRelease (id);
			BuildService.UpdateRepositories (false);
			return View ("Index", m);
		}

		public ActionResult RejectRelease (int id)
		{
			UserModel m = UserModel.GetCurrent ();
			m.CheckIsAdmin ();
			m.RejectRelease (id);
			BuildService.UpdateRepositories (false);
			return View ("Index", m);
		}
	}
}
