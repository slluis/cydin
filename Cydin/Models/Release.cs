using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cydin.Properties;
using Cydin.Builder;

namespace Cydin.Models
{
	public class Release
	{
		[DataMember (Identity = true)]
		public int Id { get; set; }

		[DataMember]
		public int ProjectId { get; set; }

		[DataMember]
		public string Version { get; set; }

		[DataMember]
		public string AddinId { get; set; }

		[DataMember]
		public string AddinName { get; set; }

		[DataMember]
		public string AddinDescription { get; set; }
		
		[DataMember]
		public string TargetAppVersion { get; set; }

		[DataMember]
		public int? SourceTagId { get; set; }

		[DataMember]
		public string Platforms { get; set; }

		[DataMember]
		public string Status { get; set; }

		[DataMember]
		public DateTime LastChangeTime { get; set; }

		[DataMember]
		public DevStatus DevStatus { get; set; }

		public string[] PlatformsList
		{
			get
			{
				if (Platforms == null)
					return new string[0];
				return Platforms.Split (new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			}
		}

		public string FileFolder
		{
			get
			{
				return Path.GetFullPath (Path.Combine (BuildService.PackagesPath, SourceTagId.ToString ()));
			}
		}

		public string GetFilePath (string platform)
		{
			string path = Path.Combine (FileFolder, platform + ".mpack");
			if (File.Exists (path))
				return path;
			else
				return Path.Combine (FileFolder, "All.mpack");
		}

		public string GetVirtualPath (string platform)
		{
			return "/Project/ReleasePackage/" + Id + "?platform=" + platform;
		}
		
		public string GetInstallerVirtualPath ()
		{
			return "/Project/ReleasePackageInstaller/" + Id;
		}
		
		public string GetPublishedVirtualPath (string platform)
		{
			return "/" + DevStatus + "/" + platform + "/" + TargetAppVersion + "/" + AddinId + "-" + Version + ".mpack";
		}
		
		public string GetReleasePackageId (string platform)
		{
			return platform + "/" + TargetAppVersion + "/" + AddinId + "-" + Version;
		}
	}

	public class ReleaseStatus
	{
		public const string Published = "Published";
		public const string PendingPublish = "Publishing";
		public const string PendingReview = "Pending";
		public const string Rejected = "Rejected";
		public const string Deleted = "Deleted";
	}
}