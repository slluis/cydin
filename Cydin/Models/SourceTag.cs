using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Cydin.Properties;
using Cydin.Builder;
using System.Text;

namespace Cydin.Models
{
	public class SourceTag
	{
		[DataMember (Identity = true)]
		public int Id { get; set; }

		[DataMember]
		public int ProjectId { get; set; }

		[DataMember]
		public int SourceId { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string AddinVersion { get; set; }

		[DataMember]
		public string AddinId { get; set; }

		[DataMember]
		public string TargetAppVersion { get; set; }

		[DataMember]
		public string Platforms { get; set; }

		[DataMember]
		public string Status { get; set; }

		[DataMember]
		public string Url { get; set; }

		[DataMember]
		public string LastRevision { get; set; }

		[DataMember]
		public DevStatus DevStatus { get; set; }

		[DataMember]
		public DateTime BuildDate { get; set; }
		
		public string ShortLastRevision {
			get {
				if (string.IsNullOrEmpty (LastRevision) || LastRevision.Length <= 15)
					return LastRevision;
				else
					return LastRevision.Substring (0, 15);
			}
		}
		
		public bool IsUpload {
			get { return Url == "(Uploaded)"; }
			set {
				if (value)
					Url = "(Uploaded)";
			}
		}
		
		public IEnumerable<string> PlatformsList
		{
			get
			{
				if (Platforms == null)
					return new string[0];
				return Platforms.Split (new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			}
		}

		public string PackagesPath
		{
			get
			{
				return Path.GetFullPath (Path.Combine (BuildService.PackagesPath, Id.ToString ()));
			}
		}

		public string LogFile
		{
			get
			{
				return Path.Combine (PackagesPath, "log.html");
			}
		}

		public string LogFileVirtualPath
		{
			get
			{
				return "/Project/BuildLog/" + Id;
			}
		}

		internal void CleanPackages ()
		{
			if (Directory.Exists (PackagesPath))
				Directory.Delete (PackagesPath, true);
		}


		public string GetFilePath (string platform)
		{
			string path = Path.Combine (PackagesPath, platform + ".mpack");
			if (File.Exists (path))
				return path;
			else
				return Path.Combine (PackagesPath, "All.mpack");
		}

		public string GetVirtualPath (string platform)
		{
			return "/Project/SourceTagPackage/" + Id + "?platform=" + platform;
		}
	}
}