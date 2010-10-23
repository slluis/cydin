
using System;
using System.Web;
using System.Web.Services;
using System.Linq;
using Cydin.Models;
using Cydin.Builder;
using System.Collections.Generic;
using Cydin.Properties;
using System.Xml.Serialization;

namespace Cydin
{
	public class Server: System.Web.Services.WebService
	{
		[WebMethod]
		public bool ConnectBuildService ()
		{
			return BuildService.Connect ();
		}
		
		[WebMethod]
		public SettingsInfo GetSettings ()
		{
			BuildService.CheckClient ();
			SettingsInfo settings = new SettingsInfo ();
			using (ServiceModel sm = ServiceModel.GetCurrent ()) {
				List<ApplicationInfo> apps = new List<ApplicationInfo> ();
				foreach (Application app in sm.GetApplications ())
					apps.Add (new ApplicationInfo () { Id = app.Id, Name = app.Name, Platforms = app.Platforms });
				settings.Applications = apps.ToArray ();
			}
			return settings;
		}
		
		[WebMethod]
		public void DisconnectBuildService ()
		{
			BuildService.CheckClient ();
			BuildService.Disconnect ();
		}
		
		[WebMethod]
		public void SetBuildServiceStatus (string status)
		{
			BuildService.CheckClient ();
			BuildService.Status = status;
		}
		
		[WebMethod]
		public void Log (LogSeverity severity, string message)
		{
			BuildService.CheckClient ();
			BuildService.Log (severity, message);
		}
		
		[WebMethod]
		public SourceInfo[] GetSources (int appId)
		{
			BuildService.CheckClient ();
			List<SourceInfo> list = new List<SourceInfo> ();
			using (UserModel m = UserModel.GetAdmin (appId)) {
				foreach (Project p in m.GetProjects ()) {
					foreach (VcsSource s in m.GetSources (p.Id)) {
						List<SourceTagInfo> tags = new List<SourceTagInfo> ();
						foreach (SourceTag st in m.GetVcsSourceTags (s.Id))
							tags.Add (new SourceTagInfo (st));
						list.Add (new SourceInfo (p, s, tags.ToArray ()));
					}
				}
				return list.ToArray ();
			}
		}
		
		public SourceTagInfo[] GetSourceTags (int appId)
		{
			BuildService.CheckClient ();
			using (UserModel m = UserModel.GetAdmin (appId)) {
				List<SourceTagInfo> list = new List<SourceTagInfo> ();
				foreach (SourceTag st in m.GetSourceTags ())
					list.Add (new SourceTagInfo (st));
				return list.ToArray ();
			}
		}
		
		[WebMethod]
		public ReleaseInfo[] GetReleases (int appId)
		{
			BuildService.CheckClient ();
			using (UserModel m = UserModel.GetAdmin (appId)) {
				List<ReleaseInfo> list = new List<ReleaseInfo> ();
				
				foreach (Project p in m.GetProjects ()) {
					foreach (Release rel in m.GetProjectReleases (p.Id))
						list.Add (new ReleaseInfo (rel));
				}
				return list.ToArray ();
			}
		}
		
		[WebMethod]
		public void SetSourceStatus (int appId, int sourceId, string status, string errorMessage)
		{
			BuildService.CheckClient ();
			using (UserModel m = UserModel.GetAdmin (appId)) {
				m.SetSourceStatus (sourceId, status, errorMessage);
			}
		}
		
		[WebMethod]
		public void SetSourceTagStatus (int appId, int sourceId, string status)
		{
			BuildService.CheckClient ();
			using (UserModel m = UserModel.GetAdmin (appId)) {
				SourceTag stag = m.GetSourceTag (sourceId);
				m.SetSourceTagStatus (stag, status);
			}
		}
		
		[WebMethod]
		public void UpdateSourceTags (int appId, int sourceId, DateTime fetchTime, SourceTagInfo[] sourceTags)
		{
			BuildService.CheckClient ();
			using (UserModel m = UserModel.GetAdmin (appId)) {
				VcsSource s = m.GetSource (sourceId);
				s.LastFetchTime = fetchTime;
				m.UpdateSource (s);
				IEnumerable<SourceTag> currentTags = m.GetVcsSourceTags (sourceId);
				foreach (SourceTagInfo stInfo in sourceTags) {
					SourceTag st = currentTags.FirstOrDefault (t => t.Url == stInfo.Url);
					if (st != null) {
						stInfo.MergeTo (st);
						m.UpdateSourceTag (st);
					}
					else {
						st = new SourceTag ();
						st.SourceId = s.Id;
						st.ProjectId = s.ProjectId;
						stInfo.MergeTo (st);
						m.CreateSourceTag (st);
					}
				}
				foreach (SourceTag st in currentTags) {
					if (!sourceTags.Any (t => t.Url == st.Url))
						m.DeleteSourceTag (st);
				}
			}
		}
		
		[WebMethod]
		public void SetPublished (int appId, int releaseId)
		{
			BuildService.CheckClient ();
			using (UserModel m = UserModel.GetAdmin (appId)) {
				m.SetPublished (m.GetRelease (releaseId));
			}
		}
		
		[WebMethod]
		public void SetSourceTagBuilt (int appId, int sourceTagId)
		{
			BuildService.CheckClient ();
			using (UserModel m = UserModel.GetAdmin (appId)) {
				SourceTag st = m.GetSourceTag (sourceTagId);
				m.SetSourceTagStatus (st, SourceTagStatus.Ready);
				VcsSource vcs = m.GetVcsSource (st.SourceId);
				if (vcs.AutoPublish)
					Builder.BuildService.PublishRelease (m, st, false);
			}
		}
		
		[WebMethod]
		public AppReleaseInfo[] GetAppReleases (int appId)
		{
			BuildService.CheckClient ();
			List<AppReleaseInfo> list = new List<AppReleaseInfo> ();
			using (UserModel m = UserModel.GetAdmin (appId)) {
				foreach (AppRelease r in m.GetAppReleases ()) {
					list.Add (new AppReleaseInfo (r));
				}
				return list.ToArray ();
			}
		}
		
		[WebMethod]
		public void SetSourceTagBuildData (int appId, int stagId, SourceTagAddinInfo[] addins)
		{
			BuildService.CheckClient ();
			SourceTagAddinInfo addin = addins [0];
			using (UserModel m = UserModel.GetAdmin (appId)) {
				SourceTag st = m.GetSourceTag (stagId);
				st.AddinVersion = addin.AddinVersion;
				st.AddinId = addin.AddinId;
				st.TargetAppVersion = addin.AppVersion;
				st.Platforms =addin.Platforms;// string.Join (" ", addin.Platforms);
				st.Status = SourceTagStatus.Built;
				st.BuildDate = DateTime.Now;
				m.UpdateSourceTag (st);
			}
		}
	}
	
	public class SourceInfo
	{
		public SourceInfo ()
		{
		}
		
		public SourceInfo (Project p, VcsSource s, SourceTagInfo[] stags)
		{
			Id = s.Id;
			ProjectName = p.Name;
			Type = s.Type;
			Url = s.Url;
			Tags = s.Tags;
			Branches = s.Branches;
			LastFetchTime = s.LastFetchTime;
			AutoPublish = s.AutoPublish;
			Directory = s.Directory;
			SourceTags = stags;
		}
		
		public int Id { get; set; }
		public string ProjectName { get; set; }
		public string Type { get; set; }
		public string Url { get; set; }
		public string Tags { get; set; }
		public string Branches { get; set; }
		public SourceTagInfo[] SourceTags { get; set; }
		public DateTime LastFetchTime { get; set; }
		public bool AutoPublish { get; set; }
		public string Directory { get; set; }
	}
	
	public class SourceTagInfo
	{
		public SourceTagInfo ()
		{
		}
		
		public SourceTagInfo (SourceTag st)
		{
			Id = st.Id;
			Name = st.Name;
			LastRevision = st.LastRevision;
			Url = st.Url;
			Status = st.Status;
		}
		
		public void MergeTo (SourceTag st)
		{
			st.Name = Name;
			st.LastRevision = LastRevision;
			st.Url = Url;
			st.Status = Status;
		}

		public int Id { get; set; }
		public string Name { get; set; }
		public string LastRevision { get; set; }
		public string Url { get; set; }
		public string Status { get; set; }
	}
	
	[XmlType ("AddinData")]
	public class SourceTagAddinInfo
	{
		public string AddinVersion { get; set; }
		public string AddinId { get; set; }
		public string AppVersion { get; set; }
		public string Platforms { get; set; }
	}
	
	public class ReleaseInfo
	{
		public ReleaseInfo ()
		{
		}
		
		public ReleaseInfo (Release rel)
		{
			Id = rel.Id;
			AddinId = rel.AddinId;
			Version = rel.Version;
			TargetAppVersion = rel.TargetAppVersion;
			Platforms = rel.PlatformsList;
			Status = rel.Status;
		}


		public int Id { get; set; }
		public string AddinId { get; set; }
		public string Version { get; set; }
		public string TargetAppVersion { get; set; }
		public string[] Platforms { get; set; }
		public string Status { get; set; }
	}
	
	public class AppReleaseInfo
	{
		public AppReleaseInfo ()
		{
		}
		
		public AppReleaseInfo (AppRelease r)
		{
			Id = r.Id;
			AppVersion = r.AppVersion;
			LastUpdateTime = r.LastUpdateTime;
			ZipUrl = "/Project/AppReleasePackage/" + Id;
		}

		public int Id { get; set; }
		public string AppVersion { get; set; }
		public string ZipUrl { get; set; }
		public DateTime LastUpdateTime { get; set; }
	}
	
	public class SettingsInfo
	{
		public ApplicationInfo[] Applications { get; set; }
	}
	
	public class ApplicationInfo
	{
		public string Name { get; set; }
		public int Id { get; set; }
		public string Platforms { get; set; }
	}
}

