using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CydinBuildService
{
	public class Settings
	{
		public static Settings Default;
		
		string dataPath;
		string msbuildCommand;
		string webSiteUrl;
		
		static Settings ()
		{
			try {
				string file = "cydin.config";
				if (!File.Exists (file)) {
					file = Path.Combine ("/etc/","cydin.config");
					if (!File.Exists (file)) {
						file = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), ".config");
						file = Path.Combine (file, "cydin.config");
						if (!File.Exists (file)) {
							Default = new Settings ();
							return;
						}
					}
				}
				XmlSerializer ser = new XmlSerializer (typeof(Settings));
				using (StreamReader sr = new StreamReader (file)) {
					Default = (Settings) ser.Deserialize (sr);
				}
			} catch (Exception ex) {
				LogService.WriteLine (ex);
				Default = new Settings ();
			}
		}
		
		public Settings ()
		{
			LiveEventsConnection = true;
			PollWaitMinutes = 5;
		}
		
		public void Dump ()
		{
			LogService.WriteLine ("Web Server: {0}", WebSiteUrl);
			LogService.WriteLine ("Data Path: {0}", DataPath);
			LogService.WriteLine ("MSBuild command: {0}", MSBuildCommand);
			LogService.WriteLine ("Poll wait: {0}", PollWaitMinutes + "m");
			LogService.WriteLine ("Live Events Connection: {0}", LiveEventsConnection);
		}
		
		[XmlElementAttribute]
		public string DataPath {
			get {
				string p = dataPath ?? "Files";
				if (Path.IsPathRooted (p))
					return p;
				else
					return Path.Combine (Environment.CurrentDirectory, p);
			}
			set { dataPath = value; }
		}
		
		public string AppReleasesPath {
			get { return Path.Combine (DataPath, "AppReleases"); }
		}
		
		public string SourcesPath {
			get { return Path.Combine (DataPath, "Source"); }
		}
		
		public string PackagesPath {
			get { return Path.Combine (DataPath, "Packages"); }
		}
		
		public string WorkAreaPath {
			get { return Path.Combine (DataPath, "WorkArea"); }
		}
		
		[XmlElementAttribute]
		public string MSBuildCommand {
			get { return msbuildCommand ?? "xbuild"; }
			set { msbuildCommand = value; }
		}
		
		[XmlElementAttribute]
		public string WebSiteUrl {
			get { return webSiteUrl; }
			set { webSiteUrl = value; }
		}

		[XmlElementAttribute]
		public string LocalAppInstallPath { get; set; }

		[XmlElementAttribute]
		public bool LiveEventsConnection { get; set; }
		
		[XmlElementAttribute]
		public int PollWaitMinutes { get; set; }
		
		[XmlElementAttribute]
		public bool AppArmorSandbox { get; set; }
	}
}

