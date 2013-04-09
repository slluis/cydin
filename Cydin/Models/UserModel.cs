using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using Cydin.Builder;
using Cydin.Properties;
using ICSharpCode.SharpZipLib.Zip;
using Mono.Addins;
using Mono.Addins.Description;
using Mono.Addins.Setup;
using System.Web.Mvc;
#if CYDIN_ON_SQLITE
using MySqlConnection = Mono.Data.Sqlite.SqliteConnection;
#else
using MySql.Data.MySqlClient;
#endif

namespace Cydin.Models
{
	public class UserModel: IDisposable
	{
		User user;
		MySqlConnection db;
		HashSet<int> ownedProjects;
		Application application;
		bool isAdmin;
		StatsModel stats;

		public static UserModel GetCurrent ()
		{
			return GetForUser (HttpContext.Current.User.Identity.Name, null, -1);
		}
		
		public static UserModel GetForUser (string login, string password, int appId)
		{
			UserModel m = new UserModel ();
			
			m.db = DataConnection.GetConnection ();
			
			if (appId != -1) {
				m.application = m.db.SelectObjectById<Application> (appId);
			}
			else if (Settings.Default.SupportsMultiApps) {
				string app = GetCurrentAppName ();
				if (app != null)
					m.application = m.db.SelectObjectWhere<Application> ("Subdomain={0}", app);
			}
			else {
				m.application = m.db.SelectObjects<Application> ().FirstOrDefault ();
			}
			
			ServiceModel sm = ServiceModel.GetCurrent ();
			if (!string.IsNullOrEmpty (login)) {
				m.user = sm.GetUser (login);
				if (password != null && !m.user.CheckPassword (password))
					m.user = null;
			}
			sm.Dispose ();
			
			if (m.application != null && m.user != null) {
				UserApplication uap = m.db.SelectObjectWhere<UserApplication> ("UserId={0} AND ApplicationId={1}", m.user.Id, m.application.Id);
				m.isAdmin = (uap != null && (uap.Permissions & ApplicationPermission.Administer) != 0);
			} else
				m.isAdmin = false;
			
			return m;
		}
		
		internal static UserModel GetAdmin (int applicationId)
		{
			UserModel m = new UserModel ();
			m.db = DataConnection.GetConnection ();
			m.user = new User ();
			m.user.IsAdmin = true;
			m.application = m.db.SelectObjectById<Application> (applicationId);
			return m;
		}

		public static Settings GetSettings ()
		{
			using (var db = DataConnection.GetConnection ()) {
				return db.ReadSettings<Settings> ();
			}
		}
		
		public static string GetCurrentAppName ()
		{
			if (Settings.Default.OperationMode == OperationMode.MultiAppDomain) {
				string app = HttpContext.Current.Request.Url.Host;
				if (app.All (c => char.IsDigit (c) || c == '.'))
					return null;
				else if (HttpContext.Current.Request.Url.Authority.EndsWith ("." + Settings.Default.WebSiteHost)) {
					int i = app.IndexOf ('.');
					if (i != -1)
						return app.Substring (0, i);
				}
			}
			else {
				string app = HttpContext.Current.Request.Url.PathAndQuery;
				int i = app.IndexOf ('/', 1);
				if (i != -1)
					return app.Substring (1, i - 1);
				else
					return app.Substring (1);
			}
			return null;
		}
		
		public void Dispose ()
		{
			db.Dispose ();
		}

		private UserModel ()
		{
		}
		
		public StatsModel Stats {
			get {
				if (stats == null)
					stats = new StatsModel (this, db);
				return stats;
			}
		}

		public User User
		{
			get { return user; }
		}

		public bool IsAdmin
		{
			get
			{
				return isAdmin || IsSiteAdmin;
			}
		}
		
		public bool IsSiteAdmin
		{
			get
			{
				return user != null && user.IsAdmin;
			}
		}
		
		public Application CurrentApplication
		{
			get {
				return application;
			}
		}
		
		HashSet<int> OwnedProjects
		{
			get
			{
				if (ownedProjects == null) {
					ownedProjects = new HashSet<int> ();
					if (user != null)
						ownedProjects.UnionWith (db.SelectObjectsWhere<UserProject> ("UserId = {0} AND Permissions & {1} != 0", user.Id, (int)ProjectPermission.Administer).Select (up => up.ProjectId));
				}
				return ownedProjects;
			}
		}

		public bool CanManageProject (Project pr)
		{
			return CanManageProject (pr.Id);
		}
		
		public bool CanManageProject (int projectId)
		{
			if (user == null)
				return false;
			return IsAdmin || OwnedProjects.Contains (projectId);
		}
		
		public void UpdateSettings (Settings s)
		{
			if (!Settings.Default.InitialConfiguration)
				CheckIsSiteAdmin ();
			db.WriteSettings (s);
		}
		
		public User GetUser (int id)
		{
			if (id == user.Id || IsSiteAdmin)
				return db.SelectObjectById<User> (id);
			else
				throw new InvalidOperationException ("Not authorized");
		}

		public void UpdateUser (User user)
		{
			if (user.Id != User.Id && !IsSiteAdmin)
				throw new InvalidOperationException ("Not authorized");
			db.UpdateObject (user);
		}

		public IEnumerable<Project> GetProjects ()
		{
			return db.SelectObjectsWhere<Project> ("ApplicationId={0}", application.Id);
		}

		public IEnumerable<Project> GetUserProjects ()
		{
			if (user == null)
				return new Project[0];

			return db.SelectObjects<Project> ("SELECT * FROM Project, UserProject WHERE Project.Id = UserProject.ProjectId AND UserProject.UserId = {0} AND UserProject.Permissions & {1} != 0 AND Project.ApplicationId={2}", user.Id, (int)ProjectPermission.Administer, application.Id);
		}
		
		public IEnumerable<User> GetApplicationAdministrators ()
		{
			return db.SelectObjects<User> ("SELECT User.* FROM User, UserApplication WHERE User.Id = UserApplication.UserId AND UserApplication.ApplicationId = {0} AND UserApplication.Permissions & {1} != 0", application.Id, (int)ApplicationPermission.Administer);
		}
		
		public void SetUserApplicationPermission (int userId, ApplicationPermission perms, bool enable)
		{
			UserApplication up = db.SelectObjectWhere<UserApplication> ("UserId={0} AND ApplicationId={1}", userId, application.Id);
			if (up == null) {
				if (enable) {
					up = new UserApplication () { UserId = userId, ApplicationId = application.Id, Permissions = perms };
					db.InsertObject (up);
				}
			}
			else {
				if (enable)
					up.Permissions |= ApplicationPermission.Administer;
				else
					up.Permissions &= ~ApplicationPermission.Administer;
				db.UpdateObject (up);
			}
		}

		public IEnumerable<User> GetProjectOwners (Project p)
		{
			return db.SelectObjects<User> ("SELECT User.* FROM User, UserProject, Project WHERE Project.Id = UserProject.ProjectId AND User.Id = UserProject.UserId AND UserProject.ProjectId = {0} AND UserProject.Permissions & {1} != 0 AND Project.ApplicationId={2}", p.Id, (int)ProjectPermission.Administer, application.Id);
		}
		
		public void AddProjectOwner (int projectId, int userId)
		{
			UserProject up = db.SelectObjectWhere<UserProject> ("UserId={0} AND ProjectId={1}", userId, projectId);
			if (up == null) {
				up = new UserProject () { UserId = userId, ProjectId = projectId, Permissions = ProjectPermission.Administer };
				db.InsertObject<UserProject> (up);
			}
			else {
				up.Permissions |= ProjectPermission.Administer;
				db.UpdateObject (up);
			}
		}

		public void RemoveProjectOwner (int projectId, int userId)
		{
			UserProject up = db.SelectObjectWhere<UserProject> ("UserId={0} AND ProjectId={1}", userId, projectId);
			if (up != null) {
				up.Permissions &= ~ProjectPermission.Administer;
				db.UpdateObject (up);
			}
		}

		public Project GetProject (int id)
		{
			return db.SelectObjectById<Project> (id);
		}

		public Project GetProjectByName (string name)
		{
			return db.SelectObjectWhere<Project> ("Name={0}", name);
		}

		public void CreateProject (Project p)
		{
			p.ApplicationId = application.Id;
			db.InsertObject (p);
			UserProject up = new UserProject ();
			up.UserId = user.Id;
			up.ProjectId = p.Id;
			up.Permissions = ProjectPermission.Administer;
			db.InsertObject (up);
			
			string subject = "Project created: " + p.Name;
			StringBuilder msg = new StringBuilder ();
			msg.AppendLine ("A new project has been created.");
			msg.AppendLine ();
			msg.AppendLine ("Name: " + p.Name);
			msg.AppendLine ("Created by: " + user.Name + " <" + user.Email + ">");
			msg.AppendLine ();
			msg.AppendFormat ("[Go to {0} Project Page]({1}).\n", p.Name, GetProjectUrl (p.Id));
			msg.AppendLine ();
			msg.AppendLine ("---");
			msg.AppendLine ();
			msg.AppendLine (p.Description);
			
			SendMail (subject, msg.ToString (), ApplicationNotification.NewProject);
		}

		public void UpdateProject (Project p)
		{
			ValidateProject (p.Id);
			db.UpdateObject (p);
			
			string subject = "Project modified: " + p.Name;
			StringBuilder msg = new StringBuilder ();
			msg.AppendLine ("A project has been modified.");
			msg.AppendLine ();
			msg.AppendLine ("Name: " + p.Name);
			msg.AppendLine ("Modified by: " + user.Name + " <" + user.Email + ">");
			msg.AppendLine ();
			msg.AppendFormat ("[Go to {0} Project Page]({1}).\n", p.Name, GetProjectUrl (p.Id));
			msg.AppendLine ();
			msg.AppendLine ("---");
			msg.AppendLine ();
			msg.AppendLine (p.Description);
			
			SendMail (subject, msg.ToString (), p.Id, ProjectNotification.DescriptionChage, ApplicationNotification.ProjectDescriptionChage);
		}

		public void ValidateProject (int projectId)
		{
			if (user == null || (!IsAdmin && !OwnedProjects.Contains (projectId)))
				throw new Exception ("Invalid project id");
		}

		public IEnumerable<VcsSource> GetSources (int projectId)
		{
			ValidateProject (projectId);
			return db.SelectObjectsWhere<VcsSource> ("ProjectId = {0}", projectId);
		}

		public VcsSource GetSource (int sourceId)
		{
			VcsSource s = db.SelectObjectById<VcsSource> (sourceId);
			if (s != null) {
				ValidateProject (s.ProjectId);
				return s;
			}
			else
				return null;
		}

		public void CreateSource (VcsSource source)
		{
			source.LastFetchTime = DateTime.Now.Subtract (TimeSpan.FromDays (1));
			db.InsertObject (source);
			BuildService.Build (CurrentApplication.Id, source.ProjectId);
		}

		public void UpdateSource (VcsSource s, bool rebuild)
		{
			VcsSource os = db.SelectObjectById<VcsSource> (s.Id);
			db.UpdateObject (s);
			
			if (os.Directory != s.Directory) {
				foreach (SourceTag st in GetVcsSourceTags (s.Id)) {
					st.Status = SourceTagStatus.Waiting;
					db.UpdateObject (st);
				}
			}
			
			if (rebuild)
				BuildService.Build (CurrentApplication.Id, os.ProjectId);
		}

		public Release GetRelease (int releaseId)
		{
			Release rel = db.SelectObjectById<Release> (releaseId);
			ValidateProject (rel.ProjectId);
			return rel;
		}

		public IEnumerable<Release> GetReleases ()
		{
			return db.SelectObjects<Release> ();
		}

		public void DeleteRelease (Release rel)
		{
			ValidateProject (rel.ProjectId);
			bool requiresRebuild = rel.Status == ReleaseStatus.Published;
			if (db.SelectObjectWhere<ReleasePackage> ("ReleaseId = {0} AND Downloads != 0", rel.Id) != null) {
				// If the release has download information, don't remove it, just mark it as deleted
				rel = db.SelectObjectById<Release> (rel.Id);
				rel.Status = ReleaseStatus.Deleted;
				db.UpdateObject (rel);
			}
			else {
				// No downloads. Delete it all
				db.DeleteObject (rel);
			}
			if (requiresRebuild)
				BuildService.UpdateRepositories (false);
		}

		public Release PublishRelease (int sourceId)
		{
			SourceTag source = db.SelectObjectById<SourceTag> (sourceId);
			ValidateProject (source.ProjectId);
			return BuildService.PublishRelease (this, source, IsAdmin);
		}
		
		static string uploadPath;

		public static string UploadPath
		{
			get
			{
				if (uploadPath != null)
					return uploadPath;

				string path = BuildService.PackagesPath;

				if (!Directory.Exists (path))
					Directory.CreateDirectory (path);
				uploadPath = path;
				return path;
			}
		}

		public SourceTag UploadRelease (int projectId, byte[] fileData, string appVersion, string[] platforms)
		{
			return UploadRelease (projectId, null, fileData, appVersion, platforms);
		}
		
		public SourceTag UploadRelease (int projectId, HttpPostedFileBase file, string appVersion, string[] platforms)
		{
			return UploadRelease (projectId, file, null, appVersion, platforms);
		}
		
		SourceTag UploadRelease (int projectId, HttpPostedFileBase file, byte[] fileData, string appVersion, string[] platforms)
		{
			if (platforms.Length == 0)
				throw new Exception ("No platform selected");
			
			VcsSource uploadSource = GetSources (projectId).Where (s => s.Type == "Upload").FirstOrDefault ();
			if (uploadSource == null) {
				uploadSource = new VcsSource ();
				uploadSource.ProjectId = projectId;
				uploadSource.Type = "Upload";
				db.InsertObject (uploadSource);
			}
			
			SourceTag st = new SourceTag ();
			st.IsUpload = true;
			st.ProjectId = projectId;
			st.SourceId = uploadSource.Id;
			st.BuildDate = DateTime.Now;
			st.Name = "Upload";
			st.Platforms = string.Join (" ", platforms);
			st.TargetAppVersion = appVersion;
			st.Status = SourceTagStatus.Ready;
			db.InsertObject (st);

			string filePath = null;
			
			if (!Directory.Exists (st.PackagesPath))
				Directory.CreateDirectory (st.PackagesPath);
			
			if (platforms.Length == CurrentApplication.PlatformsList.Length) {
				filePath = Path.Combine (st.PackagesPath, "All.mpack");
				if (file != null)
					file.SaveAs (filePath);
				else
					File.WriteAllBytes (filePath, fileData);
			}
			else {
				foreach (string plat in platforms) {
					filePath = Path.Combine (st.PackagesPath, plat + ".mpack");
					if (file != null)
						file.SaveAs (filePath);
					else
						File.WriteAllBytes (filePath, fileData);
				}
			}
			AddinInfo mpack = ReadAddinInfo (filePath);
			st.AddinId = Addin.GetIdName (mpack.Id);
			st.AddinVersion = mpack.Version;
			
			db.UpdateObject (st);
			return st;
		}

		internal static AddinInfo ReadAddinInfo (string file)
		{
			ZipFile zfile = new ZipFile (file);
			try {
				foreach (ZipEntry ze in zfile) {
					if (ze.Name == "addin.info") {
						using (Stream s = zfile.GetInputStream (ze)) {
							return AddinInfo.ReadFromAddinFile (new StreamReader (s));
						}
					}
				}
				throw new InstallException ("Addin configuration file not found in package.");
			}
			finally {
				zfile.Close ();
			}
		}


		internal void CheckIsAdmin ()
		{
			if (!IsAdmin)
				throw new Exception ("Not authorised");
		}

		internal void CheckIsSiteAdmin ()
		{
			if (!IsSiteAdmin)
				throw new Exception ("Not authorised");
		}

		public void DeleteSource (int id)
		{
			VcsSource s = db.SelectObjectById<VcsSource> (id);
			if (s != null) {
				ValidateProject (s.ProjectId);
				foreach (SourceTag st in GetVcsSourceTags (s.Id))
					DeleteSourceTag (st);
				db.DeleteObject (s);
			}
		}

		public SourceTag GetSourceTag (int sourceTagId)
		{
			SourceTag s = db.SelectObjectById<SourceTag> (sourceTagId);
			ValidateProject (s.ProjectId);
			return s;
		}

		public IEnumerable<SourceTag> GetSourceTags ()
		{
			return db.SelectObjects<SourceTag> ();
		}

		public void CleanSources (int sourceTagId)
		{
			SourceTag st = GetSourceTag (sourceTagId);
			st.CleanPackages ();
			st.Status = SourceTagStatus.Waiting;
			db.UpdateObject (st);
		}

		public void CreateSourceTag (SourceTag stag)
		{
			db.InsertObject (stag);
		}

		public void UpdateSourceTag (SourceTag stag)
		{
			db.UpdateObject (stag);
		}

		public void DeleteSourceTag (SourceTag stag)
		{
			ValidateProject (stag.ProjectId);
			stag.CleanPackages ();
			db.DeleteObject (stag);
		}

		public IEnumerable<AppRelease> GetAppReleases ()
		{
			return db.SelectObjectsWhere<AppRelease> ("ApplicationId={0}", application.Id);
		}

		public AppRelease GetAppRelease (int id)
		{
			CheckIsAdmin ();
			return db.SelectObjectById<AppRelease> (id);
		}

		public AppRelease GetAppReleaseByVersion (string version)
		{
			CheckIsAdmin ();
			return db.SelectObjectWhere<AppRelease> ("ApplicationId = {0} && AppVersion = {1}", application.Id, version);
		}
		
		public void DeleteAppRelease (int id)
		{
			CheckIsAdmin ();
			db.DeleteObject (GetAppRelease (id));
		}

		internal void CreateAppRelease (AppRelease release, HttpPostedFileBase file)
		{
			CheckIsAdmin ();
			release.ApplicationId = application.Id;
			release.LastUpdateTime = DateTime.Now;
			db.InsertObject (release);
			UpdateAppReleaseFile (release, file);
		}
		
		internal void UpdateAppRelease (AppRelease release, HttpPostedFileBase file)
		{
			CheckIsAdmin ();
			release.LastUpdateTime = DateTime.Now;
			db.UpdateObject (release);
			UpdateAppReleaseFile (release, file);
		}

		void UpdateAppReleaseFile (AppRelease release, HttpPostedFileBase file)
		{
			if (file != null) {
				string filePath = release.ZipPath;
				string dir = Path.GetDirectoryName (filePath);
	
				if (!Directory.Exists (dir))
					Directory.CreateDirectory (dir);
				
				file.SaveAs (filePath);
			}
		}

		void SaveFile (Stream inStream, string path)
		{
			byte[] buffer = new byte[8192];
			int n = 0;
			Stream outStream = null;
			try {
				outStream = File.Create (path);
				while ((n = inStream.Read (buffer, 0, buffer.Length)) > 0)
					outStream.Write (buffer, 0, n);
			}
			finally {
				inStream.Close ();
				if (outStream != null)
					outStream.Close ();
			}
		}

		public IEnumerable<Release> GetPendingReleases (string status)
		{
			return db.SelectObjectsWhere<Release> ("Status = {0}", status);
		}

		internal void ApproveRelease (int id)
		{
			CheckIsAdmin ();
			Release rel = GetRelease (id);
			rel.Status = ReleaseStatus.PendingPublish;
			db.UpdateObject (rel);
		}

		internal void RejectRelease (int id)
		{
			CheckIsAdmin ();
			Release rel = GetRelease (id);
			rel.Status = ReleaseStatus.Rejected;
			db.UpdateObject (rel);
		}

		public void SetProjectNotification (ProjectNotification notif, int projectId, bool enable)
		{
			if ((notif & ProjectNotification.OwnerNotificationMask) != 0)
				ValidateProject (projectId);
			
			if ((notif & ProjectNotification.AdminNotificationMask) != 0)
				CheckIsAdmin ();
			
			UserProject up = db.SelectObject<UserProject> ("SELECT * FROM UserProject WHERE UserId={0} AND ProjectId={1}", user.Id, projectId);
			if (enable) {
				if (up == null) {
					up = new UserProject () { UserId = user.Id, ProjectId = projectId, Notifications = notif };
					db.InsertObject<UserProject> (up);
				}
				else {
					up.Notifications |= notif;
					db.UpdateObject (up);
				}
			} else if (up != null) {
				up.Notifications &= ~notif;
				db.UpdateObject (up);
			}
		}

		public void SetApplicationNotification (ProjectNotification notif, bool enable)
		{
			if ((notif & ProjectNotification.AdminNotificationMask) != 0)
				CheckIsAdmin ();
			
			UserApplication up = db.SelectObjectWhere<UserApplication> ("UserId={0} AND ApplicationId={1}", user.Id, application.Id);
			if (enable) {
				if (up == null) {
					up = new UserApplication () { UserId = user.Id, ApplicationId = application.Id, ProjectNotifications = notif };
					db.InsertObject<UserApplication> (up);
				}
				else {
					up.ProjectNotifications |= notif;
					db.UpdateObject (up);
				}
			} else if (up != null) {
				up.ProjectNotifications &= ~notif;
				db.UpdateObject (up);
			}
		}

		public void SetApplicationNotification (ApplicationNotification notif, bool enable)
		{
			if ((notif & ApplicationNotification.AdminNotificationMask) != 0)
				CheckIsAdmin ();
			
			UserApplication up = db.SelectObjectWhere<UserApplication> ("UserId={0} AND ApplicationId={1}", user.Id, application.Id);
			if (enable) {
				if (up == null) {
					up = new UserApplication () { UserId = user.Id, ApplicationId = application.Id, Notifications = notif };
					db.InsertObject<UserApplication> (up);
				}
				else {
					up.Notifications |= notif;
					db.UpdateObject (up);
				}
			} else if (up != null) {
				up.Notifications &= ~notif;
				db.UpdateObject (up);
			}
		}

		public void SetSiteNotification (SiteNotification notif, bool enable)
		{
			if ((notif & SiteNotification.SiteAdminNotificationMask) != 0)
				CheckIsSiteAdmin ();
			
			if (enable)
				User.SiteNotifications |= notif;
			else
				User.SiteNotifications &= ~notif;
			
			db.UpdateObject (User);
		}

		public void SetUserPermission (ApplicationPermission perm)
		{
			CheckIsAdmin ();
			UserApplication up = db.SelectObjectWhere<UserApplication> ("UserId={0} AND ApplicationId={1}", user.Id, application.Id);
			if (up == null) {
				up = new UserApplication () { UserId = user.Id, ApplicationId = application.Id, Permissions = perm };
				db.InsertObject<UserApplication> (up);
			}
			else {
				up.Permissions |= perm;
				db.UpdateObject (up);
			}
		}

		public void ResetUserPermission (ApplicationPermission perm)
		{
			CheckIsAdmin ();
			UserApplication up = db.SelectObjectWhere<UserApplication> ("UserId={0} AND ApplicationId={1}", user.Id, application.Id);
			if (up != null) {
				up.Permissions &= ~perm;
				db.UpdateObject (up);
			}
		}

		public IEnumerable<Release> GetRecentReleases ()
		{
			return db.SelectObjects<Release> ("SELECT `Release`.* FROM `Release`, Project WHERE `Release`.ProjectId = Project.Id AND Project.ApplicationId = {0} AND `Release`.Status = {1} AND `Release`.DevStatus != {2} ORDER BY LastChangeTime DESC", application.Id, ReleaseStatus.Published, DevStatus.Test).Take (10);
		}

		internal void UpdateProjectFlags (int id, ProjectFlag flags)
		{
			CheckIsAdmin ();
			Project p = GetProject (id);
			p.Flags = flags;
			db.UpdateObject (p);
		}

		internal void SetProjectFlags (int id, ProjectFlag flags)
		{
			CheckIsAdmin ();
			Project p = GetProject (id);
			p.Flags |= flags;
			db.UpdateObject (p);
		}

		internal void ResetProjectFlags (int id, ProjectFlag flags)
		{
			CheckIsAdmin ();
			Project p = GetProject (id);
			p.Flags &= ~flags;
			db.UpdateObject (p);
		}

		internal void DeleteProject (int id)
		{
			ValidateProject (id);
			Project p = GetProject (id);
			foreach (Release rel in db.SelectObjectsWhere<Release> ("ProjectId={0}", p.Id)) {
				db.DeleteObject (rel);
			}
			foreach (SourceTag stag in db.SelectObjectsWhere<SourceTag> ("ProjectId={0}", p.Id)) {
				stag.CleanPackages ();
				db.DeleteObject (stag);
			}
			foreach (VcsSource s in db.SelectObjectsWhere<VcsSource> ("ProjectId={0}", p.Id)) {
				db.DeleteObject (s);
			}
			db.DeleteObject (p);
			
			string subject = "Project deleted: " + p.Name;
			StringBuilder msg = new StringBuilder ();
			msg.AppendLine ("The project '" + p.Name + "' has been deleted by " + user.Name + "(" + user.Email + ").");
			SendMail (subject, msg.ToString (), ApplicationNotification.DeleteProject);
		}

		public void SetPublished (Release rel)
		{
			rel.Status = ReleaseStatus.Published;
			rel.LastChangeTime = DateTime.Now;
			db.UpdateObject (rel);
			
			Project p = GetProject (rel.ProjectId);
			bool firstRelease = !p.IsPublic;
			if (!p.IsPublic) {
				p.IsPublic = true;
				db.UpdateObject (p);
			}
			
			if (rel.DevStatus != DevStatus.Test) {
				string subject = "New add-in release published: " + rel.AddinId + " v" + rel.Version;
				StringBuilder msg = new StringBuilder ();
				msg.AppendLine ("The add-in " + rel.AddinId + " v" + rel.Version + " has been released.");
				msg.AppendLine ();
				msg.AppendFormat ("[Go to {0} Project Page]({1}).\n", p.Name, GetProjectUrl (p.Id));
				
				if (firstRelease)
					SendMail (subject, msg.ToString (), p.Id, ProjectNotification.NewRelease, ApplicationNotification.ProjectNewRelease, ApplicationNotification.FirstProjectRelease);
				else
					SendMail (subject, msg.ToString (), p.Id, ProjectNotification.NewRelease, ApplicationNotification.ProjectNewRelease);
			}
		}

		public Release GetPublishedRelease (SourceTag st)
		{
			return db.SelectObjectWhere<Release> ("SourceTagId = {0}", st.Id);
		}

		public void CreateRelease (Release rel)
		{
			Release oldRel = db.SelectObjectWhere<Release> ("ProjectId={0} AND Version={1} AND AddinId={2} AND TargetAppVersion={3} AND Status={4}", rel.ProjectId, rel.Version, rel.AddinId, rel.TargetAppVersion, ReleaseStatus.Deleted);
			if (oldRel != null) {
				rel.Id = oldRel.Id;
				db.UpdateObject (rel);
			}
			else
				db.InsertObject (rel);
			
			BindDownloadInfo (rel);
			
			Project p = GetProject (rel.ProjectId);
			
			if (rel.Status == ReleaseStatus.PendingReview) {
				string subject = "Add-in release review pending: " + rel.AddinId + " v" + rel.Version;
				StringBuilder msg = new StringBuilder ();
				msg.AppendLine ("The user '" + user.Name + "' has requested the publication of the release of the add-in " + rel.AddinId + " v" + rel.Version + ".");
				msg.AppendLine ();
				msg.AppendFormat ("[Go to {0} Project Page]({1}).\n", p.Name, GetProjectUrl (p.Id));
				msg.AppendFormat ("[Go to Review Page]({0}/Review).", GetBaseUrl ());
				SendMail (subject, msg.ToString (), p.Id, ProjectNotification.PublishReleaseRequest, ApplicationNotification.ProjectPublishReleaseRequest);
			}
		}
		
		string GetProjectUrl (int projectId)
		{
			return GetBaseUrl () + "/Project/Index/" + projectId;
		}
		
		string GetBuildLogUrl (int stagId)
		{
			return GetBaseUrl () + "/Project/BuildLog/" + stagId;
		}
		
		string GetBaseUrl ()
		{
			return "http://" + Settings.Default.WebSiteHost;
		}
		
		public void BindDownloadInfo (Release rel)
		{
			foreach (string plat in rel.PlatformsList) {
				// Make sure there is at least one ReleasePackage register for this platform
				string fid = rel.GetReleasePackageId (plat);
				ReleasePackage rp = db.SelectObjectWhere<ReleasePackage> ("FileId={0}", fid);
				if (rp == null) {
					rp = new ReleasePackage ();
					rp.ReleaseId = rel.Id;
					rp.FileId = fid;
					rp.Date = DateTime.Now;
					rp.TargetAppVersion = rel.TargetAppVersion;
					rp.Platform = plat;
					rp.Downloads = 0;
					db.InsertObject (rp);
				}
			}
		}

		public void SetSourceTagStatus (SourceTag stag, string status)
		{
			bool statusChanged = false;
			stag.Status = status;

			if (status != SourceTagStatus.Building && status != SourceTagStatus.Fetching && status != SourceTagStatus.Waiting) {
				// A final status
				statusChanged = stag.LastStatus != status;
				stag.LastStatus = status;
			}

			stag.BuildDate = DateTime.Now;
			db.UpdateObject (stag);
			
			Project p = GetProject (stag.ProjectId);

			if (status == SourceTagStatus.BuildError && statusChanged) {
				string subject = "Project build failed: " + p.Name + " (" + stag.Name + ")";
				StringBuilder msg = new StringBuilder ();
				msg.AppendLine ("The project **" + p.Name + "** failed to build.");
				msg.AppendLine ();
				msg.AppendLine ("Tag/Branch: " + stag.Name);
				msg.AppendLine ("Revision: " + stag.LastRevision);
				msg.AppendLine ();
				msg.AppendFormat ("[Go to Project Page]({0}).\n", GetProjectUrl (p.Id));
				msg.AppendFormat ("[View Build Log]({0}).\n", GetBuildLogUrl (stag.Id));
				SendMail (subject, msg.ToString (), p.Id, ProjectNotification.BuildError, ApplicationNotification.ProjectBuildError);
			}
			else if (status == SourceTagStatus.Built) {
				string subject = "Project built: " + p.Name + " (" + stag.Name + ")";
				StringBuilder msg = new StringBuilder ();
				msg.AppendLine ("The project **" + p.Name + "** has been successfuly built.");
				msg.AppendLine ();
				msg.AppendLine ("Tag/Branch: " + stag.Name);
				msg.AppendLine ("Revision: " + stag.LastRevision);
				msg.AppendLine ("Add-in version: " + stag.AddinVersion);
				msg.AppendLine ("App version: " + stag.TargetAppVersion);
				msg.AppendLine ();
				msg.AppendFormat ("[Go to Project Page]({0}).\n", GetProjectUrl (p.Id));
				msg.AppendFormat ("[View Build Log]({0}).\n", GetBuildLogUrl (stag.Id));
				SendMail (subject, msg.ToString (), p.Id, ProjectNotification.BuildSuccess, ApplicationNotification.ProjectBuildSuccess);
			}
			else if (status == SourceTagStatus.FetchError && statusChanged) {
				string subject = "Project fetch failed: " + p.Name + " (" + stag.Name + ")";
				StringBuilder msg = new StringBuilder ();
				msg.AppendLine ("The source code of the project **" + p.Name + "** could not be fetched.");
				msg.AppendLine ();
				msg.AppendLine ("Tag/Branch: " + stag.Name);
				msg.AppendLine ("Revision: " + stag.LastRevision);
				msg.AppendLine ();
				msg.AppendFormat ("[Go to Project Page]({0}).\n", GetProjectUrl (p.Id));
				SendMail (subject, msg.ToString (), p.Id, ProjectNotification.BuildError, ApplicationNotification.ProjectBuildError);
			}
		}
		
		public IEnumerable<SourceTag> GetVcsSourceTags (int sourceId)
		{
			return db.SelectObjectsWhere<SourceTag> ("SourceId = {0}", sourceId);
		}

		public IEnumerable<VcsSource> GetProjectSources (int projectId)
		{
			return db.SelectObjectsWhere<VcsSource> ("ProjectId = {0}", projectId);
		}

		public IEnumerable<SourceTag> GetProjectSourceTags (int projectId)
		{
			return db.SelectObjectsWhere<SourceTag> ("ProjectId = {0}", projectId);
		}

		public VcsSource GetVcsSource (int id)
		{
			return db.SelectObjectById<VcsSource> (id);
		}

		public IEnumerable<Release> GetProjectReleases (int projectId)
		{
			return db.SelectObjectsWhere<Release> ("ProjectId={0} AND Status != {1}", projectId, ReleaseStatus.Deleted);
		}

		public IEnumerable<Application> GetApplications ()
		{
			return db.SelectObjects<Application> ();
		}
		
		public Application GetApplication (int id)
		{
			return db.SelectObjectById<Application> (id);
		}
		
		internal void SetSourceStatus (int id, string status)
		{
			SetSourceStatus (id, status, null);
		}

		internal void SetSourceStatus (int id, string status, string message)
		{
			VcsSource s = GetSource (id);
			s.Status = status;
			if (message != null)
				s.ErrorMessage = message;
			db.UpdateObject (s);
		}
		
		public List<SelectListItem> GetDevStatusItems (DevStatus current)
		{
			List<SelectListItem> items = new List<SelectListItem> ();
			items.Add (new SelectListItem () { Text = "Stable", Value = ((int) DevStatus.Stable).ToString (), Selected = current == DevStatus.Stable });
			items.Add (new SelectListItem () { Text = "Beta", Value = ((int) DevStatus.Beta).ToString (), Selected = current == DevStatus.Beta });
			items.Add (new SelectListItem () { Text = "Alpha", Value = ((int) DevStatus.Alpha).ToString (), Selected = current == DevStatus.Alpha });
			items.Add (new SelectListItem () { Text = "Test", Value = ((int) DevStatus.Test).ToString (), Selected = current == DevStatus.Test });
			return items;
		}
		
		public List<NotificationInfo> GetProjectNotifications (int projectId)
		{
			ProjectNotification notifs = (ProjectNotification) 0;
			UserProject up = db.SelectObjectWhere<UserProject> ("UserId={0} AND ProjectId={1}", User.Id, projectId);
			if (up != null)
				notifs = up.Notifications;
			
			List<NotificationInfo> list = new List<NotificationInfo> ();
			list.Add (new NotificationInfo ("New Add-in Release", ProjectNotification.NewRelease, notifs));
			
			if (CanManageProject (projectId)) {
				list.Add (new NotificationInfo ("Build Success", ProjectNotification.BuildSuccess, notifs));
				list.Add (new NotificationInfo ("Build Error", ProjectNotification.BuildError, notifs));
			}
			
			if (IsAdmin) {
				list.Add (new NotificationInfo ("Description Changed", ProjectNotification.DescriptionChage, notifs));
				list.Add (new NotificationInfo ("Release Publish Request", ProjectNotification.PublishReleaseRequest, notifs));
			}
			return list;
		}
		
		public List<NotificationInfo> GetApplicationNotifications ()
		{
			ApplicationNotification notifs = (ApplicationNotification) 0;
			ProjectNotification projectNotifs = (ProjectNotification) 0;
			
			UserApplication uap = db.SelectObjectWhere<UserApplication> ("UserId={0} AND ApplicationId={1}", User.Id, application.Id);
			if (uap != null) {
				notifs = uap.Notifications;
				projectNotifs = uap.ProjectNotifications;
			}
			
			List<NotificationInfo> list = new List<NotificationInfo> ();
			bool hasProjects = OwnedProjects.Count > 0;
			
			if (hasProjects)
				list.Add (new NotificationInfo ("Global Notifications"));
				
			list.Add (new NotificationInfo ("New Add-in", ApplicationNotification.FirstProjectRelease, notifs));
			list.Add (new NotificationInfo ("New Add-in Release", ApplicationNotification.ProjectNewRelease, notifs));
			
			if (IsAdmin) {
				list.Add (new NotificationInfo ("Build Success", ApplicationNotification.ProjectBuildSuccess, notifs));
				list.Add (new NotificationInfo ("Build Error", ApplicationNotification.ProjectBuildError, notifs));
				list.Add (new NotificationInfo ("Project Created", ApplicationNotification.NewProject, notifs));
				list.Add (new NotificationInfo ("Project Deleted", ApplicationNotification.DeleteProject, notifs));
				list.Add (new NotificationInfo ("Description Changed", ApplicationNotification.ProjectDescriptionChage, notifs));
				list.Add (new NotificationInfo ("Release Publish Request", ApplicationNotification.ProjectPublishReleaseRequest, notifs));
			}
			
			if (IsSiteAdmin) {
				list.Add (new NotificationInfo ("New User", SiteNotification.NewUser, User.SiteNotifications));
				list.Add (new NotificationInfo ("Build Bot Error", SiteNotification.BuildBotError, User.SiteNotifications));
			}
				
			if (hasProjects) {
				list.Add (new NotificationInfo ("Owned Project Notifications"));
				list.Add (new NotificationInfo ("Release Published", ProjectNotification.NewRelease, projectNotifs));
				list.Add (new NotificationInfo ("Build Success", ProjectNotification.BuildSuccess, projectNotifs));
				list.Add (new NotificationInfo ("Build Error", ProjectNotification.BuildError, projectNotifs));
			}
			return list;
		}
		
		public void SendMail (string subject, string body, int projectId, params object[] notif)
		{
			HashSet<string> users = new HashSet<string> ();
			foreach (object not in notif) {
				if (not is ProjectNotification)
					users.UnionWith (GetUsersToNotify (projectId, (ProjectNotification) not));
				else if (not is ApplicationNotification)
					users.UnionWith (GetUsersToNotify ((ApplicationNotification) not));
			}
			BuildService.SendMail (users, subject, body);
		}
		
		public void SendMail (string subject, string body, ApplicationNotification notif)
		{
			BuildService.SendMail (GetUsersToNotify (notif), subject, body);
		}
		
		public void SendMail (string subject, string body, ProjectNotification notif, int projectId)
		{
			BuildService.SendMail (GetUsersToNotify (projectId, notif), subject, body);
		}
		
		IEnumerable<string> GetUsersToNotify (ApplicationNotification notif)
		{
			foreach (var user in db.SelectObjects<User> ("SELECT User.* FROM User, UserApplication WHERE UserApplication.UserId = User.Id AND UserApplication.ApplicationId={0} AND UserApplication.Notifications & {1} = {2}", application.Id, (int)notif, (int)notif))
				yield return user.Email;
		}

		IEnumerable<string> GetUsersToNotify (int projectId, ProjectNotification notif)
		{
			HashSet<string> emails = new HashSet<string> ();
			
			// Users with the notification flag for the project
			var users = db.SelectObjects<User> (
				"SELECT User.* FROM User, UserProject WHERE " +
				"User.Id = UserProject.UserId AND " +
				"UserProject.ProjectId={0} AND Notifications & {1} = {2}", projectId, (int)notif, (int)notif);
			emails.UnionWith (users.Select (u => u.Email));
	
			// Notification to be sent to project owners
			users = db.SelectObjects<User> (
				"SELECT User.* FROM User, UserApplication, UserProject WHERE " +
				"User.Id = UserApplication.UserId AND " +
				"User.Id = UserProject.UserId AND " +
				"UserProject.ProjectId = {0} AND " +
				"UserApplication.ApplicationId = {1} AND " +
				"UserProject.Permissions & 1 = 1 AND " +
				"UserApplication.ProjectNotifications & {2} = {3}", projectId, application.Id, (int)notif, (int)notif);
			emails.UnionWith (users.Select (u => u.Email));
			
			return emails;
		}
	}
	
	public class DataConnection
	{
		public static MySqlConnection GetConnection ()
		{
			MySqlConnection db = null;
			try {
				string conn_string = WebConfigurationManager.ConnectionStrings["CommunityAddinRepoConnectionString"].ConnectionString;
				db = new MySqlConnection (conn_string);
				db.Open ();
				return db;
			} catch (Exception ex) {
				if (db != null)
					db.Close ();
				throw new Exception ("Database connection failed", ex);
			}
		}
	}
	
	public class NotificationInfo
	{
		public NotificationInfo (string name)
		{
			Name = name;
		}
		
		public NotificationInfo (string name, object value, object enabled)
		{
			this.Name = name;
			this.Value = value;
			this.Id = "notify-" + value.ToString ();
			int v1 = (int) Convert.ChangeType (value, typeof(int));
			int v2 = (int) Convert.ChangeType (enabled, typeof(int));
			Enabled = (v1 & v2) != 0;
		}
		
		public bool IsGroup {
			get { return Value == null; }
		}
		
		public string Name { get; internal set; }
		public string Id { get; internal set; }
		public bool Enabled { get; internal set; }
		public object Value { get; internal set; }
	}
	
	public enum TimePeriod
	{
		Day,
		Week,
		Month,
		Year,
		All,
		Auto
	}
}
