using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CydinBuildService
{
	public class SourceTagStatus
	{
		public const string Waiting = "Waiting";
		public const string Fetching = "Fetching";
		public const string Building = "Building";
		public const string FetchError = "Fetch Error";
		public const string BuildError = "Build Error";
		public const string Ready = "Ready";
	}
}