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
		
    }
}
