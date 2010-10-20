using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Cydin.Properties;
using Cydin.Builder;

namespace Cydin.Models
{
	public class UserProject
	{
		[DataMember (Key = true)]
		public int UserId { get; set; }
		
		[DataMember (Key = true)]
		public int ProjectId { get; set; }
		
		[DataMember]
		public ProjectPermission Permissions { get; set; }
		
		[DataMember]
		public ProjectNotification Notifications { get; set; }
	}
}

