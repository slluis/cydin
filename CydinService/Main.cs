using System;
using System.Runtime.InteropServices;
using CydinBuildService;
using System.Text;

namespace CydinService
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Starting Cydin service");
			SetProcessName ("cydind");
			BuildService buildBot = new BuildService ();
			buildBot.Start (null);
			Console.WriteLine ("Running");
		}
		
		public static void SetProcessName (string name)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				try {
					unixSetProcessName (name);
				} catch (Exception e) {
					Console.WriteLine ("Error setting process name", e);
				}
			}
		}
		
		[DllImport ("libc")] // Linux
		private static extern int prctl (int option, byte [] arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);
		
		[DllImport ("libc")] // BSD
		private static extern void setproctitle (byte [] fmt, byte [] str_arg);
		
		//this is from http://abock.org/2006/02/09/changing-process-name-in-mono/
		static void unixSetProcessName (string name)
		{
			try {
				if (prctl (15 /* PR_SET_NAME */, Encoding.ASCII.GetBytes (name + "\0"), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) != 0) {
					throw new ApplicationException ("Error setting process name.");
				}
			} catch (EntryPointNotFoundException) {
				// Not every BSD has setproctitle
				try {
					setproctitle (Encoding.ASCII.GetBytes ("%s\0"), Encoding.ASCII.GetBytes (name + "\0"));
				} catch (EntryPointNotFoundException) {}
			}
		}
	}
}

