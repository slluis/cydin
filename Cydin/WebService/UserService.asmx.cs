using System;
using System.Linq;
using System.Web.Services;
using Cydin.Models;
using System.Collections.Generic;

namespace Cydin
{
	public class UserService : System.Web.Services.WebService
	{
		UserModel GetUserModel (LoginInfo login)
		{
			return UserModel.GetForUser (login.User, login.Password ?? "", login.AppId);
		}
		
		[WebMethod]
		public LoginInfo Login (string user, string password, string applicationName)
		{
			LoginInfo login = new LoginInfo ();
			login.User = user;
			login.Password = password;
			login.AppId = -1;
			
			using (UserModel m = GetUserModel (login)) {
				var app = m.GetApplications ().FirstOrDefault (a => a.Name == applicationName);
				if (app != null)
					login.AppId = app.Id;
			}
			return login;
		}
		
		[WebMethod]
		public int GetProjectId (LoginInfo login, string name)
		{
			using (UserModel m = GetUserModel (login)) {
				var p = m.GetProjectByName (name);
				return p != null ? p.Id : -1;
			}
		}
		
		int GetProjectId (UserModel m, string name)
		{
			var p = m.GetProjectByName (name);
			if (p == null)
				throw new Exception ("Project not found: " + name);
			return p.Id;
		}
		
		[WebMethod]
		public ReleaseInfo[] GetProjectReleases (LoginInfo login, string projectName)
		{
			using (UserModel m = GetUserModel (login)) {
				return m.GetProjectReleases (GetProjectId(m, projectName)).Select (r => new ReleaseInfo (r)).ToArray ();
			}
		}
		
		[WebMethod]
		public SourceTagAddinInfo[] GetProjectSourceTags (LoginInfo login, string projectName)
		{
			using (UserModel m = GetUserModel (login)) {
				return m.GetProjectSourceTags (GetProjectId (m, projectName)).Select (r => new SourceTagAddinInfo (r)).ToArray ();
			}
		}
		
		[WebMethod]
		public AppReleaseInfo[] GetAppReleases (LoginInfo login, int appId)
		{
			List<AppReleaseInfo> list = new List<AppReleaseInfo> ();
			using (UserModel m = GetUserModel (login)) {
				foreach (AppRelease r in m.GetAppReleases ()) {
					list.Add (new AppReleaseInfo (r));
				}
				return list.ToArray ();
			}
		}
		
		[WebMethod]
		public void CreateAppRelease (LoginInfo login, string version, string addinsVersion, string compatibleVersion)
		{
			using (UserModel m = GetUserModel (login)) {
				AppRelease rel = new AppRelease ();
				rel.AppVersion = version;
				rel.AddinRootVersion = addinsVersion;
				var compatRel = m.GetAppReleases ().FirstOrDefault (r => r.AppVersion == compatibleVersion);
				if (compatRel != null)
					rel.CompatibleAppReleaseId = compatRel.Id;
				m.CreateAppRelease (rel, null);
			}
		}
		
		[WebMethod]
		public int UploadAddin (LoginInfo login, string projectName, string targetAppVersion, string[] platforms, byte[] fileContent)
		{
			using (UserModel m = GetUserModel (login)) {
				int pid = GetProjectId (m, projectName);
				m.ValidateProject (pid);
				return m.UploadRelease (pid, fileContent, targetAppVersion, platforms).Id;
			}
		}
		
		[WebMethod]
		public void PublishAddin (LoginInfo login, int addinSourceId, DevStatus devStatus)
		{
			using (UserModel m = GetUserModel (login)) {
				var st = m.GetSourceTag (addinSourceId);
				if (st.DevStatus != devStatus) {
					st.DevStatus = devStatus;
					m.UpdateSourceTag (st);
				}
				m.PublishRelease (addinSourceId);
			}
		}
	}
}

