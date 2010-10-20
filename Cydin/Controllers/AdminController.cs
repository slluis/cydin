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
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
			UserModel m = UserModel.GetCurrent ();
			m.CheckIsAdmin ();
			return View (m);
        }

		public ActionResult UpdateRepositories ()
		{
			UserModel m = UserModel.GetCurrent ();
			m.CheckIsAdmin ();
			BuildService.UpdateRepositories (true);
			return View ("Index", m);
		}
		
		public ActionResult ProjectsList ()
		{
			UserModel m = UserModel.GetCurrent ();
			m.CheckIsAdmin ();
			return View (m);
		}
		
    }
}
