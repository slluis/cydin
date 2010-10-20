using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cydin.Models;

namespace Cydin.Controllers
{
    public class UserHomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

		public ActionResult NewProject ()
		{
			return View (new Project ());
		}
    }
}
