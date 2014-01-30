using System;
using System.Linq;
using System.Web.Services;
using Cydin.Models;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
	
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
		public AppReleaseInfo[] GetAppReleases (LoginInfo login)
		{
			List<AppReleaseInfo> list = new List<AppReleaseInfo> ();
			using (UserModel m = GetUserModel (login)) {
				foreach (AppRelease r in m.GetAppReleases ()) {
					list.Add (new AppReleaseInfo (m, r));
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
				if (!string.IsNullOrEmpty (compatibleVersion)) {
					var compatRel = m.GetAppReleases ().FirstOrDefault (r => r.AppVersion == compatibleVersion);
					if (compatRel != null)
						rel.CompatibleAppReleaseId = compatRel.Id;
					else
						throw new Exception ("Invalid compatible release number. Release '" + compatibleVersion + "' not found");
				}
				m.CreateAppRelease (rel, null);
				Cydin.Builder.BuildService.UpdateRepositories (true);
			}
		}
		
		[WebMethod]
		public void UpdateAppRelease (LoginInfo login, AppReleaseInfo appRelease)
		{
			using (UserModel m = GetUserModel (login)) {
				var arel = m.GetAppRelease (appRelease.Id);
				if (arel == null)
					throw new Exception ("Release not found: " + appRelease.Id);
				arel.AppVersion = appRelease.AppVersion;
				if (!string.IsNullOrEmpty (appRelease.CompatibleAppVersion)) {
					var compatRel = m.GetAppReleases ().FirstOrDefault (r => r.AppVersion == appRelease.CompatibleAppVersion);
					if (compatRel != null)
						arel.CompatibleAppReleaseId = compatRel.Id;
					else
						throw new Exception ("Invalid compatible release number. Release '" + appRelease.CompatibleAppVersion + "' not found");
				} else
					arel.CompatibleAppReleaseId = null;
				
				m.UpdateAppRelease (arel, null);
				Cydin.Builder.BuildService.UpdateRepositories (true);
			}
		}
		
		[WebMethod]
		public void DeleteAppRelease (LoginInfo login, AppReleaseInfo appRelease)
		{
			using (UserModel m = GetUserModel (login)) {
				var arel = m.GetAppRelease (appRelease.Id);
				m.DeleteAppRelease (arel.Id);
			}
		}
		
		[WebMethod]
		public SourceTagAddinInfo UploadAddin (LoginInfo login, string projectName, string targetAppVersion, string[] platforms, byte[] fileContent)
		{
			using (UserModel m = GetUserModel (login)) {
				int pid = GetProjectId (m, projectName);
				m.ValidateProject (pid);
				return new SourceTagAddinInfo (m.UploadRelease (pid, fileContent, targetAppVersion, platforms));
			}
		}
		
		[WebMethod]
		public SourceTagAddinInfo GetAddinInfo (LoginInfo login, string projectName, string targetAppVersion, string addinId, string version)
		{
			using (UserModel m = GetUserModel (login)) {
				int pid = GetProjectId (m, projectName);
				m.ValidateProject (pid);
				var s = m.GetProjectSourceTags (pid).FirstOrDefault (st => st.TargetAppVersion == targetAppVersion && st.AddinVersion == version && st.AddinId == addinId);
				if (s != null)
					return new SourceTagAddinInfo (s);
				else
					return null;
			}
		}

		[WebMethod]
		public ReleaseInfo PublishAddin (LoginInfo login, int addinSourceId, DevStatus devStatus)
		{
			using (UserModel m = GetUserModel (login)) {
				var st = m.GetSourceTag (addinSourceId);
				if (st.DevStatus != devStatus) {
					st.DevStatus = devStatus;
					m.UpdateSourceTag (st);
				}
				return new ReleaseInfo (m.PublishRelease (addinSourceId));
			}
		}
		
		[WebMethod]
		public string GetSourceTagPackageHash (LoginInfo login, int sourceId, string platform)
		{
			using (UserModel m = GetUserModel (login)) {
				var rel = m.GetSourceTag (sourceId);
				string file = rel.GetFilePath (platform);
				using (var f = System.IO.File.OpenRead (file)) {
					MD5 md5 = MD5.Create ();
					byte[] hash = md5.ComputeHash (f);
					StringBuilder sb = new StringBuilder ();
					foreach (byte b in hash)
						sb.Append (b.ToString ("x"));
					return sb.ToString ();
				}
			}
		}
		
		[WebMethod]
		public void RemoveAddinSourceTag (LoginInfo login, int addinSourceId)
		{
			using (UserModel m = GetUserModel (login)) {
				var st = m.GetSourceTag (addinSourceId);
				m.DeleteSourceTag (st);
			}
		}

		[WebMethod]
		public string GetReleasePackageHash (LoginInfo login, int releaseId, string platform)
		{
			using (UserModel m = GetUserModel (login)) {
				var rel = m.GetRelease (releaseId);
				string file = rel.GetFilePath (platform);
				using (var f = System.IO.File.OpenRead (file)) {
					MD5 md5 = MD5.Create ();
					byte[] hash = md5.ComputeHash (f);
					return Convert.ToBase64String (hash);
				}
			}
		}
	}
}

