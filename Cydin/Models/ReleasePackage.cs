using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cydin.Properties;
using Cydin.Builder;

namespace Cydin.Models
{
	public class ReleasePackage
	{
		[DataMember]
		public int ReleaseId { get; set; }

		[DataMember (Key=true)]
		public string FileId { get; set; }

		[DataMember]
		public string Platform { get; set; }

		[DataMember]
		public string TargetAppVersion { get; set; }

		[DataMember]
		public int Downloads { get; set; }

		[DataMember (Key=true)]
		public DateTime Date { get; set; }
	}
}