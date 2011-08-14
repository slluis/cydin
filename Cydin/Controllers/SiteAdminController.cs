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
	public class SiteAdminController: CydinController
	{
        public ActionResult Index()
        {
			CurrentUserModel.CheckIsSiteAdmin ();
			return View ();
        }
		
        public ActionResult Setup()
        {
			if (Settings.Default.OperationMode != OperationMode.NotSet) {
				if (!CurrentServiceModel.ThereIsAdministrator ())
					return Redirect (ControllerHelper.GetActionUrl ("home", "Login", "User"));
				else
					RedirectToAction ("Index", "Home");
			}
			return View ();
        }
		
		public ActionResult EnableServiceChange ()
		{
			CurrentUserModel.CheckIsSiteAdmin ();
			BuildService.AllowChangingService = true;
			return View ("Index");
		}
		
		public ActionResult DisableServiceChange ()
		{
			CurrentUserModel.CheckIsSiteAdmin ();
			BuildService.AllowChangingService = false;
			return View ("Index");
		}
		
		public ActionResult AuthorizeServiceChange ()
		{
			CurrentUserModel.CheckIsSiteAdmin ();
			BuildService.AuthorizeServiceConnection (BuildService.BuildBotConnectionRequest);
			return View ("Index");
		}
		
		public ActionResult EditSettings ()
		{
			Settings s = UserModel.GetSettings ();
			if (!Settings.Default.InitialConfiguration) {
				CurrentUserModel.CheckIsSiteAdmin ();
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
			if (!Settings.Default.InitialConfiguration)
				CurrentUserModel.CheckIsSiteAdmin ();
			
			Settings.Default.DataPath = s.DataPath;
			Settings.Default.OperationMode = s.OperationMode;
			Settings.Default.WebSiteHost = s.WebSiteHost;
			Settings.Default.SmtpHost = s.SmtpHost;
			Settings.Default.SmtpPassword = s.SmtpPassword;
			Settings.Default.SmtpPort = s.SmtpPort;
			Settings.Default.SmtpUser = s.SmtpUser;
			Settings.Default.SmtpUseSSL = s.SmtpUseSSL;
			
			CurrentUserModel.UpdateSettings (Settings.Default);
			
			Cydin.MvcApplication.UpdateRoutes ();
			if (!CurrentServiceModel.ThereIsAdministrator ())
				return Redirect (ControllerHelper.GetActionUrl ("home", "Login", "User"));
			else {
				CurrentServiceModel.EndInitialConfiguration ();
				return Redirect (ControllerHelper.GetActionUrl ("home", null, null));
			}
        }
		
		public ActionResult NewApplication ()
		{
			CurrentUserModel.CheckIsSiteAdmin ();
			Application app = new Application ();
			app.Id = -1;
			return View ("EditApplication", app);
		}
		
		public ActionResult EditApplication (int id)
		{
			CurrentUserModel.CheckIsSiteAdmin ();
			Application app = CurrentServiceModel.GetApplication (id);
			return View ("EditApplication", app);
		}
		
		[HttpPost]
        public ActionResult UpdateApplication (Application app)
        {
			UserModel m = CurrentUserModel;
			m.CheckIsSiteAdmin ();
			if (app.Id != -1) {
				Application capp = CurrentServiceModel.GetApplication (app.Id);
				capp.Name = app.Name;
				capp.Subdomain = app.Subdomain;
				capp.Platforms = app.Platforms;
				CurrentServiceModel.UpdateApplication (capp);
			}
			else {
				app.Description = "<p>This is the home page of the add-in repository for " + app.Name + "</p><p>Click on the 'Edit Page' link to change the content of this welcome page</p>";
				CurrentServiceModel.CreateApplication (app);
			}
			return RedirectToAction ("Index");
        }
		
		public ActionResult UsersList ()
		{
			CurrentUserModel.CheckIsSiteAdmin ();
			return View ();
		}
		
		public ActionResult Log ()
		{
			CurrentUserModel.CheckIsSiteAdmin ();
			return View ();
		}
		
		public ActionResult ClearLog ()
		{
			System.IO.File.WriteAllText (BuildService.LogFile, "");
			return RedirectToAction ("Index");
		}
		
		public ActionResult AddUser (string login, string password, string email)
		{
			CurrentUserModel.CheckIsSiteAdmin ();
			
			User u = new User ();
			u.Email = email;
			u.Login = login;
			u.Name = login;
			u.SetPassword (password);
			
			CurrentServiceModel.CreateUser (u);
			return RedirectToAction ("Index");
		}
	}
}

