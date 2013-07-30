using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Cydin.Properties;

namespace Cydin.Models
{
	public class AppRelease
	{
		[DataMember (Identity=true)]
		public int Id { get; set; }

		[DataMember]
		public int ApplicationId { get; set; }

		[DataMember]
		public string AppVersion { get; set; }

		[DataMember]
		public string AddinRootVersion { get; set; }

		[DataMember]
		public DateTime LastUpdateTime { get; set; }

		[DataMember]
		public int? CompatibleAppReleaseId { get; set; }

		public string ZipUrl { get; set; }
		
		public string ZipPath {
			get {
				string path = Path.Combine (Settings.Default.DataPath, "AppReleases");
				if (!Path.IsPathRooted (path))
					path = Path.Combine (Settings.BasePath, path);
				return Path.GetFullPath (Path.Combine (path, AppVersion + ".zip"));
			}
		}
	}
}