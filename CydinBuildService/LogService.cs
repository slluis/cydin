using System;

namespace CydinBuildService
{
	public static class LogService
	{
		public static void WriteLine (string message)
		{
			Console.WriteLine (message);
		}

		public static void WriteLine (string message, params object[] args)
		{
			Console.WriteLine (message, args);
		}

		public static void WriteLine (Exception ex)
		{
			Console.WriteLine (ex);
		}
	}
}

