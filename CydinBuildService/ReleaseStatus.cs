using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CydinBuildService
{
	public class ReleaseStatus
	{
		public const string Published = "Published";
		public const string PendingPublish = "Publishing";
		public const string PendingReview = "Pending";
		public const string Rejected = "Rejected";
	}
}