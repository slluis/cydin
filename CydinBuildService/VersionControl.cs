using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Text;
using System.Xml;
//using Cydin.Properties;
using CydinBuildService.n127_0_0_1;

namespace CydinBuildService
{
	public static class VersionControl
	{
		static List<SourcePuller> pullers = new List<SourcePuller> ();
		
		static VersionControl ()
		{
			pullers.Add (new SubversionPuller ());
			pullers.Add (new GitPuller ());
			pullers.Add (new BazaarPuller ());
		}
		
		public static SourcePuller GetSourcePuller (string type)
		{
			SourcePuller sp = pullers.FirstOrDefault (p => p.Type == type);
			if (sp == null)
				throw new InvalidOperationException ("VCS type not supported: " + type);
			return sp;
		}
	}
}