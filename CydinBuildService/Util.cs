// 
// Util.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CydinBuildService
{
	public static class Util
	{
		public static bool FindMatch (string str, List<string> list)
		{
			foreach (string t in list) {
				if (t == str || t == "*")
					return true;
				if (t.IndexOf ('*') != -1 || t.IndexOf ('?') != -1) {
					// Match wildcards
					string rx = ToRegex (t);
					if (Regex.IsMatch (str, rx))
						return true;
				}
			}
			return false;
		}
		
		static string ToRegex (string str)
		{
			char[] ec = new char[] {'.', '(', ')', '\\', '[', ']', '^', '$', '|', '+', '{', '}' };
			foreach (char c in ec)
				str = str.Replace (c.ToString (), "\\" + c.ToString ());
			return str.Replace ("*", ".*").Replace ("?",".");
		}
		
		public static void ResetFolder (string path)
		{
			if (!Directory.Exists (Path.GetDirectoryName (path)))
				Directory.CreateDirectory (Path.GetDirectoryName (path));

			if (Directory.Exists (path))
				Directory.Delete (path, true);
			
			Directory.CreateDirectory (path);
		}
		
		public static void SaveToFile (this Stream inStream, string path)
		{
			byte[] buffer = new byte[32768];
			int n = 0;
			Stream outStream = null;
			try {
				outStream = File.Create (path);
				while ((n = inStream.Read (buffer, 0, buffer.Length)) > 0)
					outStream.Write (buffer, 0, n);
			}
			finally {
				inStream.Close ();
				if (outStream != null)
					outStream.Close ();
			}
		}
		
		public static void WriteFile (this Stream outStream, string path)
		{
			byte[] buffer = new byte[32768];
			int n = 0;
			Stream inStream = null;
			try {
				inStream = File.OpenRead (path);
				while ((n = inStream.Read (buffer, 0, buffer.Length)) > 0)
					outStream.Write (buffer, 0, n);
			}
			finally {
				outStream.Close ();
				if (inStream != null)
					inStream.Close ();
			}
		}
	}
}

