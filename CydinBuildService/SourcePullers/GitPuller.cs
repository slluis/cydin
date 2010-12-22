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
	public class GitPuller: SourcePuller
	{
		public static int Timeout = 5 * 60 * 1000;
		
		public static string GitCommand {
			get { return "git"; }
		}
		
		public override string Type {
			get {
				return "GIT";
			}
		}
		public string GetGitPath (BuildContext ctx, int sourceId)
		{
			string gitDir = Path.Combine (ctx.LocalSettings.DataPath, "Git");
			if (!Directory.Exists (gitDir))
				Directory.CreateDirectory (gitDir);
			return Path.Combine (gitDir, sourceId.ToString ());
		}
		
		public override string GetSourceTagPath (BuildContext ctx, int sourceId, int sourceTagId)
		{
			return GetGitPath (ctx, sourceId);
		}
		
		public override IEnumerable<SourceTagInfo> GetChildUrls (BuildContext ctx, SourceInfo source)
		{
			// URL syntax:
			// url|t<name>,b<name>,...
			
			List<string> selBranches = new List<string> ();
			List<string> selTags = new List<string> ();
			
			if (source.Branches != null) {
				foreach (string b in source.Branches.Split (new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
					selBranches.Add (b.Trim ());
			}
			
			if (source.Tags != null) {
				foreach (string b in source.Tags.Split (new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
					selTags.Add (b.Trim ());
			}
			
			if (selBranches.Count == 0 && selTags.Count == 0)
				selBranches.Add ("master");
			
			UpdateRepo (ctx, source.Id, source.Url, null);
			
			string gitDir = GetGitPath (ctx, source.Id);
			
			foreach (string b in RunCommand (gitDir, "branch", null)) {
				if (b.Length < 3)
					continue;
				string br = b.Substring (2);
				if (Util.FindMatch (br, selBranches)) {
					RunCommand (gitDir, "checkout " + br, null);
					string rev = GetCurrentRevision (gitDir);
					yield return new SourceTagInfo () { Url = source.Url + "|b" + br, Name = br, LastRevision = rev };
				}
			}
			
			foreach (string t in RunCommand (gitDir, "tag", null)) {
				if (t.Trim ().Length == 0)
					continue;
				if (Util.FindMatch (t, selTags)) {
					RunCommand (gitDir, "checkout " + t, null);
					string rev = GetCurrentRevision (gitDir);
					yield return new SourceTagInfo () { Url = source.Url + "|t" + t, Name = t, LastRevision = rev };
				}
			}
		}
		
		public override void Fetch (BuildContext ctx, int sourceId, SourceTagInfo stag, StringBuilder output, StringBuilder error)
		{
			int i = stag.Url.IndexOf ('|');
			string bname = stag.Url.Substring (i + 2);
			string url = stag.Url.Substring (0, i);
			
			UpdateRepo (ctx, sourceId, url, output);
			
			string gitDir = GetGitPath (ctx, sourceId);
			
			RunCommand (gitDir, "checkout " + bname, output);
		}
		
		public override void PrepareForBuild (BuildContext ctx, int sourceId, SourceTagInfo stag)
		{
			int i = stag.Url.IndexOf ('|');
			string bname = stag.Url.Substring (i + 2);
			string gitDir = GetGitPath (ctx, sourceId);
			RunCommand (gitDir, "checkout " + bname, null);
		}
		
		void UpdateRepo (BuildContext ctx, int sourceId, string url, StringBuilder output)
		{
			string gitDir = GetGitPath (ctx, sourceId);
			if (!Directory.Exists (gitDir))
				RunCommand (".", "clone --depth=1 " + url + " " + gitDir, output);
			else {
				try {
					RunCommand (gitDir, "checkout master", output);
					RunCommand (gitDir, "pull", output);
				} catch (Exception ex) {
					Console.WriteLine ("Error: " + ex.Message);
					// If something goes wrong while updating, reclone
					Directory.Delete (gitDir, true);
					RunCommand (null, "clone --depth=1 " + url + " " + gitDir, output);
				}
			}
		}
		
		string GetCurrentRevision (string gitDir)
		{
			return RunCommand (gitDir, "log -1 --format=format:%H", null).First ();
		}
		
		IEnumerable<string> RunCommand (string gitDir, string cmd, StringBuilder log)
		{
			if (log != null)
				log.AppendLine ("> git " + cmd);
			Console.WriteLine ("> git " + cmd);
			StringBuilder output = new StringBuilder ();
			StringBuilder error = new StringBuilder ();
			try {
				BuildService.RunCommand (false, GitCommand, cmd, output, error, Timeout, gitDir);
			} catch (Exception ex) {
				throw new Exception (ex.Message + ": " + output.ToString () + " " + error.ToString ());
			}
			if (log != null)
				log.AppendLine (output.ToString ());
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

