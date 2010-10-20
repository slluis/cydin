using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CydinBuildService
{
	public class Settings
	{
		public static Settings DefaultS;
		
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
							DefaultS = new Settings ();
							return;
						}
					}
				}
				XmlSerializer ser = new XmlSerializer (typeof(Settings));
				using (StreamReader sr = new StreamReader (file)) {
					DefaultS = (Settings) ser.Deserialize (sr);
				}
			} catch {
				DefaultS = new Settings ();
			}
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
	}
}

