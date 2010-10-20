// 
// SubversionPuller.cs
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
using System.Xml;

namespace CydinBuildService
{
	public class SubversionPuller: SourcePuller
	{
		public static int Timeout = 5 * 60 * 1000;
		
		public static string SubversionCommand {
			get { return "svn"; }
		}
		
		public override string Type {
			get {
				return "SVN";
			}
		}
		
		public override IEnumerable<SourceTagInfo> GetChildUrls (BuildContext ctx, SourceInfo source)
		{
			StringBuilder output = new StringBuilder ();
			StringBuilder error = new StringBuilder ();
			string url = source.Url;
			
			if (!url.EndsWith ("/*")) {
				BuildService.RunCommand (SubversionCommand, "info --xml " + url, output, error, Timeout);
				XmlDocument doc = new XmlDocument ();
				doc.LoadXml (output.ToString ());
				XmlElement elem = (XmlElement) doc.SelectSingleNode ("/info/entry");
				if (elem == null) {
					elem = (XmlElement)doc.SelectSingleNode ("/info");
					if (elem != null)
						throw new Exception (elem.InnerText);
					else if (error.Length > 0)
						throw new Exception (error.ToString ());
					else
						throw new Exception ("Error while getting repository information");
				}
				yield return new SourceTagInfo () { Url = url, Name = elem.GetAttribute ("path"), LastRevision = elem.GetAttribute ("revision") };
			}
			else {
				url = url.Substring (0, url.Length - 2);
				BuildService.RunCommand (SubversionCommand, "ls --xml " + url, output, error, Timeout);
				XmlDocument doc = new XmlDocument ();
				try {
					doc.LoadXml (output.ToString ());
				}
				catch {
					if (error.Length > 0)
						throw new Exception (error.ToString ());
					else
						throw new Exception ("Error while getting repository information");
				}
				foreach (XmlElement elem in doc.SelectNodes ("/lists/list/entry")) {
					string name = elem["name"].InnerText;
					string revision = elem["commit"].GetAttribute ("revision");
					yield return new SourceTagInfo () { Url = url + "/" + name, Name = name, LastRevision = revision };

				}
			}
		}
		
		public override void Fetch (BuildContext ctx, int sourceId, SourceTagInfo stag, StringBuilder output, StringBuilder error)
		{
			string command;
			string args;
			string targetPath = GetSourceTagPath (ctx, sourceId, stag.Id);

			Util.ResetFolder (targetPath);
			command = SubversionCommand;
			args = "export --force " + stag.Url + " \"" + targetPath + "\"";

			output.AppendLine (command + " " + args);
			BuildService.RunCommand (command, args, output, error, Timeout);
		}
	}
}

