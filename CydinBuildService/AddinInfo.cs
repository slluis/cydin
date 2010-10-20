//
// AddinInfo.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Collections;
using System.Xml;
using Mono.Addins.Description;

namespace Mono.Addins.Setup
{
	internal class AddinInfo
	{
		string id = "";
		string namspace = "";
		string name = "";
		string version = "";
		string baseVersion = "";
		string author = "";
		string copyright = "";
		string url = "";
		string description = "";
		string category = "";
		DependencyCollection dependencies;
		DependencyCollection optionalDependencies;
		
		public AddinInfo ()
		{
			dependencies = new DependencyCollection ();
			optionalDependencies = new DependencyCollection ();
		}
		
		public string Id {
			get { return Addin.GetFullId (namspace, id, version); }
		}
		
		public string LocalId {
			get { return id; }
			set { id = value; }
		}
		
		public string Namespace {
			get { return namspace; }
			set { namspace = value; }
		}
		
		public string Name {
			get {
				if (name != null && name.Length > 0)
					return name;
				string sid = id;
				if (sid.StartsWith ("__"))
					sid = sid.Substring (2);
				return Addin.GetFullId (namspace, sid, null); 
			}
			set { name = value; }
		}
		
		public string Version {
			get { return version; }
			set { version = value; }
		}
		
		public string BaseVersion {
			get { return baseVersion; }
			set { baseVersion = value; }
		}
		
		public string Author {
			get { return author; }
			set { author = value; }
		}
		
		public string Copyright {
			get { return copyright; }
			set { copyright = value; }
		}
		
		public string Url {
			get { return url; }
			set { url = value; }
		}
		
		public string Description {
			get { return description; }
			set { description = value; }
		}
		
		public string Category {
			get { return category; }
			set { category = value; }
		}

		public string TargetAppVersion { get; set; }
		
		public DependencyCollection Dependencies {
			get { return dependencies; }
		}
		
		public DependencyCollection OptionalDependencies {
			get { return optionalDependencies; }
		}
		
		public static AddinInfo ReadFromAddinFile (StreamReader r)
		{
			XmlDocument doc = new XmlDocument ();
			doc.Load (r);
			r.Close ();
			
			AddinInfo info = new AddinInfo ();
			info.id = doc.DocumentElement.GetAttribute ("id");
			info.namspace = doc.DocumentElement.GetAttribute ("namespace");
			info.name = doc.DocumentElement.GetAttribute ("name");
			if (info.id == "") info.id = info.name;
			info.version = doc.DocumentElement.GetAttribute ("version");
			info.author = doc.DocumentElement.GetAttribute ("author");
			info.copyright = doc.DocumentElement.GetAttribute ("copyright");
			info.url = doc.DocumentElement.GetAttribute ("url");
			info.description = doc.DocumentElement.GetAttribute ("description");
			info.category = doc.DocumentElement.GetAttribute ("category");
			info.baseVersion = doc.DocumentElement.GetAttribute ("compatVersion");

			ReadDependencies (info.Dependencies, info.OptionalDependencies, doc.DocumentElement);
		
			return info;
		}
		
		static void ReadDependencies (DependencyCollection deps, DependencyCollection opDeps, XmlElement elem)
		{
			foreach (XmlElement dep in elem.SelectNodes ("Dependencies/Addin")) {
				AddinDependency adep = new AddinDependency ();
				adep.AddinId = dep.GetAttribute ("id");
				string v = dep.GetAttribute ("version");
				if (v.Length != 0)
					adep.Version = v;
				deps.Add (adep);
			}
			
			foreach (XmlElement dep in elem.SelectNodes ("Dependencies/Assembly")) {
				AssemblyDependency adep = new AssemblyDependency ();
				adep.FullName = dep.GetAttribute ("name");
				adep.Package = dep.GetAttribute ("package");
				deps.Add (adep);
			}
			
			foreach (XmlElement mod in elem.SelectNodes ("Module"))
				ReadDependencies (opDeps, opDeps, mod);
		}
	}
}
