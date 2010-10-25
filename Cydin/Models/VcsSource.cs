using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Common;

namespace Cydin.Models
{
	public partial class VcsSource
	{
		[DataMember (Identity = true)]
		public int Id { get; set; }

		[DataMember]
		public int ProjectId { get; set; }

		[DataMember]
		public string Url { get; set; }

		[DataMember]
		public string Type { get; set; }

		[DataMember]
		public bool AutoPublish { get; set; }

		[DataMember]
		public string Status { get; set; }

		[DataMember]
		public DateTime LastFetchTime { get; set; }

		[DataMember]
		public string ErrorMessage { get; set; }

		[DataMember]
		public int FetchFailureCount{ get; set; }

		[DataMember]
		public string Tags { get; set; }

		[DataMember]
		public string Branches { get; set; }

		[DataMember]
		public string Directory { get; set; }
		
		public bool IsUploadSource {
			get { return Type == "Upload"; }
		}
	}
}