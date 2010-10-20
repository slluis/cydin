// 
// ServiceModel.cs
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
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Text;
using Cydin.Builder;
using Cydin.Properties;
using System.IO;

namespace Cydin.Models
{
	public class ServiceModel
	{
		protected MySqlConnection db;
		string version;
		
		public static ServiceModel GetCurrent ()
		{
			ServiceModel m = new ServiceModel ();
			m.db = DataConnection.GetConnection ();
			return m;
		}
		
		protected ServiceModel ()
		{
		}
		
		internal User GetUserFromOpenId (string identifier)
		{
			return db.SelectObjectWhere<User> ("OpenId = {0}", identifier);
		}
		
		void CheckIsAdmin ()
		{
			UserModel.GetCurrent ().CheckIsAdmin ();
		}
		
		public string Version {
			get {
				if (version == null) {
					string vpath = Path.Combine (Settings.BasePath, "version");
					if (File.Exists (vpath))
						version = File.ReadAllText (vpath);
					else
						version = "Unknown";
				}
				return version;
			}
		}
		
		public IEnumerable<Application> GetApplications ()
		{
			return db.SelectObjects<Application> ();
		}
		
		public Application GetApplication (int id)
		{
			return db.SelectObjectById<Application> (id);
		}
		
		public AppRelease GetAppRelease (int id)
		{
			return db.SelectObjectById<AppRelease> (id);
		}

		internal bool IsUserNameAvailable (string p)
		{
			return GetUser (p) == null;
		}

		public IEnumerable<Project> GetUserProjects (int userId)
		{
			CheckIsAdmin ();
			return db.SelectObjects<Project> ("SELECT * FROM Project, UserProject WHERE Project.Id = UserProject.ProjectId AND UserProject.UserId = {0} AND UserProject.Permissions & {1} != 0", userId, (int)ProjectPermission.Administer);
		}
		
		public void UpdateOpenId (string oldId, string newId)
		{
			db.ExecuteCommand ("UPDATE User SET OpenId={0} WHERE OpenId={1}", newId, oldId);
		}

		public User GetUser (string login)
		{
			return db.SelectObjectWhere<User> ("Login = {0}", login);
		}

		public IEnumerable<User> GetUsers ()
		{
			CheckIsAdmin ();
			return db.SelectObjects<User> ();
		}
		
		public bool ThereIsAdministrator ()
		{
			return db.SelectObjectWhere<User> ("IsAdmin = 1") != null;
		}
		
		public void EndInitialConfiguration ()
		{
			Settings.Default.InitialConfiguration = false;
			db.WriteSettings (Settings.Default);
		}
			
		internal void UpdateUser2 (User user)
		{
			db.UpdateObject (user);
		}

		internal void CreateUser (User user)
		{
			if (Settings.Default.InitialConfiguration)
				user.IsAdmin = true;
			
			db.InsertObject (user);
			
			if (Settings.Default.InitialConfiguration)
				EndInitialConfiguration ();
			
			string subject = "User registered: " + user.Name;
			StringBuilder msg = new StringBuilder ();
			msg.AppendLine ("A new user has been registered.");
			msg.AppendLine ();
			msg.AppendLine ("Name: " + user.Name);
			msg.AppendLine ("E-mail: " + user.Email);
			SendMail (subject, msg.ToString (), SiteNotification.NewUser);
		}
		
		public void SendMail (string subject, string body, SiteNotification notif)
		{
			BuildService.SendMail (GetUsersToNotify (notif), subject, body);
		}
		
		IEnumerable<string> GetUsersToNotify (SiteNotification notif)
		{
			foreach (var user in db.SelectObjectsWhere<User> ("SiteNotifications & {0} = {1}", (int)notif, (int)notif))
				yield return user.Email;
		}
		
		public void UpdateApplication (Application app)
		{
			db.UpdateObject (app);
		}
		
		public void CreateApplication (Application app)
		{
			db.InsertObject (app);
		}
	}
}

