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
			return View (ServiceModel.GetCurrent ());
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
			
			Settings.Default.DataPath = s.DataPath;
			Settings.Default.OperationMode = s.OperationMode;
			Settings.Default.WebSiteHost = s.WebSiteHost;
			Settings.Default.SmtpHost = s.SmtpHost;
			Settings.Default.SmtpPassword = s.SmtpPassword;
			Settings.Default.SmtpPort = s.SmtpPort;
			Settings.Default.SmtpUser = s.SmtpUser;
			Settings.Default.SmtpUseSSL = s.SmtpUseSSL;
			
			m.UpdateSettings (Settings.Default);
			
			Cydin.MvcApplication.UpdateRoutes ();
			if (!ServiceModel.GetCurrent ().ThereIsAdministrator ())
				return Redirect (ControllerHelper.GetActionUrl ("home", "Login", "User"));
			else {
				ServiceModel.GetCurrent ().EndInitialConfiguration ();
				return Redirect (ControllerHelper.GetActionUrl ("home", null, null));
			}
        }
		
		public ActionResult NewApplication ()
		{
			UserModel m = UserModel.GetCurrent ();
			m.CheckIsSiteAdmin ();
			Application app = new Application ();
			app.Id = -1;
			return View ("EditApplication", app);
		}
		
		public ActionResult EditApplication (int id)
		{
			UserModel m = UserModel.GetCurrent ();
			m.CheckIsSiteAdmin ();
			Application app = m.ServiceModel.GetApplication (id);
			return View ("EditApplication", app);
		}
		
		[HttpPost]
        public ActionResult UpdateApplication (Application app)
        {
			UserModel m = UserModel.GetCurrent ();
			m.CheckIsSiteAdmin ();
			if (app.Id != -1) {
				Application capp = m.ServiceModel.GetApplication (app.Id);
				capp.Name = app.Name;
				capp.Subdomain = app.Subdomain;
				capp.Platforms = app.Platforms;
				m.ServiceModel.UpdateApplication (capp);
			}
			else {
				app.Description = "<p>This is the home page of the add-in repository for " + app.Name + "</p><p>Click on the 'Edit Page' link to change the content of this welcome page</p>";
				m.ServiceModel.CreateApplication (app);
			}
			return RedirectToAction ("Index");
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

