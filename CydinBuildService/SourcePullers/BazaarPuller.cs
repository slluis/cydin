// 
// GitPuller.cs
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
using System.Collections.Generic;
using CydinBuildService.n127_0_0_1;
using System.Text;
using System.IO;
using System.Linq;


namespace CydinBuildService
{
	public class BazaarPuller: SourcePuller
	{
		public static int Timeout = 5 * 60 * 1000;
		
		public static string BzrCommand {
			get { return "bzr"; }
		}
		
		public override string Type {
			get {
				return "BZR";
			}
		}
		public string GetBzrPath (BuildContext ctx, int sourceId)
		{
			string bzrDir = Path.Combine (ctx.LocalSettings.DataPath, "Bzr");
			if (!Directory.Exists (bzrDir))
				Directory.CreateDirectory (bzrDir);
			return Path.Combine (bzrDir, sourceId.ToString ());
		}
		
		public override string GetSourceTagPath (BuildContext ctx, int sourceId, int sourceTagId)
		{
			return GetBzrPath (ctx, sourceId);
		}
		
		public override IEnumerable<SourceTagInfo> GetChildUrls (BuildContext ctx, SourceInfo source)
		{
			List<string> selTags = new List<string> ();
			
			if (source.Tags != null) {
				foreach (string b in source.Tags.Split (new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
					selTags.Add (b.Trim ());
			}
			
			UpdateRepo (ctx, source.Id, source.Url, null);
			
			string bzrDir = GetBzrPath (ctx, source.Id);
			
			if (selTags.Count > 0) {
				foreach (string t in RunCommand (bzrDir, "tags", null)) {
					int i = t.IndexOf (' ');
					string tn = t.Substring (0, i);
					if (Util.FindMatch (t, selTags)) {
						string rev = t.Substring (i+1).Trim ();
						yield return new SourceTagInfo () { Url = source.Url + "|t" + tn, Name = tn, LastRevision = rev };
					}
				}
			} else {
				int i = source.Url.LastIndexOf ('/');
				string rev = RunCommand (bzrDir, "revno", null).FirstOrDefault () ?? "Unknown";
				yield return new SourceTagInfo () { Url = source.Url, Name = source.Url.Substring (i+1), LastRevision = rev };
			}
		}
		
		public override void Fetch (BuildContext ctx, int sourceId, SourceTagInfo stag, StringBuilder output, StringBuilder error)
		{
			string bzrDir = GetBzrPath (ctx, sourceId);
			string url = stag.Url;
			int i = stag.Url.IndexOf ('|');
			if (i != -1) {
				url = url.Substring (0, i);
				UpdateRepo (ctx, sourceId, url, output);
				string bname = stag.Url.Substring (i + 2);
				RunCommand (bzrDir, "export -r tag:" + bname + " " + base.GetSourceTagPath (ctx, sourceId, stag.Id), output);
			}
			else
				UpdateRepo (ctx, sourceId, url, output);
		}
		
		void UpdateRepo (BuildContext ctx, int sourceId, string url, StringBuilder output)
		{
			string bzrDir = GetBzrPath (ctx, sourceId);
			if (!Directory.Exists (bzrDir))
				RunCommand (".", "checkout " + url + " " + bzrDir, output);
			else {
				try {
					RunCommand (bzrDir, "update", output);
				} catch {
					// If something goes wrong while updating, reclone
					Directory.Delete (bzrDir, true);
					RunCommand (".", "checkout " + url + " " + bzrDir, output);
				}
			}
		}
				
		IEnumerable<string> RunCommand (string bzrDir, string cmd, StringBuilder log)
		{
			if (log != null)
				log.AppendLine ("> bzr " + cmd);
			Console.WriteLine ("> bzr " + cmd);
			StringBuilder output = new StringBuilder ();
			StringBuilder error = new StringBuilder ();
			try {
				BuildService.RunCommand (BzrCommand, cmd, output, error, Timeout, bzrDir);
			} catch (Exception ex) {
				throw new Exception (ex.Message + ": " + output.ToString ());
			}
			if (log != null) {
				log.AppendLine (output.ToString ());
				log.AppendLine (error.ToString ());
			}
			Console.WriteLine (output.ToString ());
			
			List<string> lines = new List<string> ();
			StringReader sr = new StringReader (output.ToString ());
			string line;
			while ((line = sr.ReadLine ()) != null)
				lines.Add (line);
			return lines;
		}
	}
}

