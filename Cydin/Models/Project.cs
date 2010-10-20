using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Common;

namespace Cydin.Models
{
	public class Project
	{
		[DataMember (Identity = true)]
		public int Id { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public bool Trusted { get; set; }

		[DataMember]
		public int ApplicationId { get; set; }

		[DataMember]
		public bool IsPublic { get; set; }
	}
}