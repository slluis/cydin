using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mono.Addins;
using System.Text;

namespace Cydin.Models
{
	class LocalStatusMonitor : IProgressStatus
	{
		StringBuilder sb = new StringBuilder ();

		public void Cancel ()
		{
		}

		public bool IsCanceled
		{
			get { return false; }
		}

		public void Log (string msg)
		{
			sb.Append (msg.Replace ("\r", "").Replace ("\n", "<br/>"));
		}

		public int LogLevel
		{
			get { return 0; }
		}

		public void ReportError (string message, Exception exception)
		{
			sb.Append ("ERROR: " + message + "<br/>");
		}

		public void ReportWarning (string message)
		{
			sb.Append ("WARNING: " + message + "<br/>");
		}

		public void SetMessage (string msg)
		{
		}

		public void SetProgress (double progress)
		{
		}

		public override string ToString ()
		{
			return sb.ToString ();
		}
	}
}