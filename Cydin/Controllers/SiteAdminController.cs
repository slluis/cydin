// 
// SiteAdminController.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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
	public class SiteAdminController: Controller
	{
        public ActionResult Index()
        {
			UserModel m = UserModel.GetCurrent ();
			m.CheckIsSiteAdmin ();
			return View (m.ServiceModel);
        }
		
        public ActionResult Setup()
        {
			if (Settings.Default.OperationMode != OperationMode.NotSet) {
				if (!ServiceModel.GetCurrent ().ThereIsAdministrator ())
					return Redirect (ControllerHelper.GetActionUrl ("home", "User", "Login"));
				else
					RedirectToAction ("Index", "Home");
			}
			UserModel m = UserModel.GetCurrent ();
			return View (m.ServiceModel);
        }
		
		public ActionResult EnableServiceChange ()
		{
			UserModel m = UserModel.GetCurrent ();
			m.CheckIsSiteAdmin ();
			BuildService.AllowChangingService = true;
			return View ("Index", m.ServiceModel);
		}
		
		public ActionResult DisableServiceChange ()
		{
			UserModel m = UserModel.GetCurrent ();
			m.CheckIsSiteAdmin ();
			BuildService.AllowChangingService = false;
			return View ("Index", m.ServiceModel);
		}
		
		public ActionResult AuthorizeServiceChange ()
		{
			UserModel m = UserModel.GetCurrent ();
			m.CheckIsSiteAdmin ();
			BuildService.AuthorizeServiceConnection (BuildService.BuildBotConnectionRequest);
			return View ("Index", m.ServiceModel);
		}
		
		public ActionResult EditSettings ()
		{
			Settings s = UserModel.GetSettings ();
			if (!Settings.Default.InitialConfiguration) {
				UserModel m = UserModel.GetCurrent ();
				m.CheckIsSiteAdmin ();
				return View ("Settings", s);
			} else {
				s.WebSiteHost = HttpContext.Request.Url.Host;
				if (HttpContext.Request.Url.Port != 80)
					s.WebSiteHost += ":" + HttpContext.Request.Url.Port;
				return View ("Settings", s);
			}
		}
		
		[HttpPost]
        public ActionResult SaveSettings (Settings s)
        {
			UserModel m = UserModel.GetCurrent ();
			if (!Settings.Default.InitialConfiguration)
				m.CheckIsSiteAdmin ();
			
			m.UpdateSettings (s);
			
			Settings.Default = s;
			Cydin.MvcApplication.UpdateRoutes ();
			if (!ServiceModel.GetCurrent ().ThereIsAdministrator ())
				return Redirect (ControllerHelper.GetActionUrl ("home", "User", "Login"));
			else {
				ServiceModel.GetCurrent ().EndInitialConfiguration ();
				return Redirect (ControllerHelper.GetActionUrl ("home", null, null));
			}
        }
		
		public ActionResult UsersList ()
		{
			UserModel m = UserModel.GetCurrent ();
			m.CheckIsSiteAdmin ();
			return View (m.ServiceModel);
		}
		
		public ActionResult Log ()
		{
			UserModel m = UserModel.GetCurrent ();
			m.CheckIsSiteAdmin ();
			return View (m.ServiceModel);
		}
	}
}

