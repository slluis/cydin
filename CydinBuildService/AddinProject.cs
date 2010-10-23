// 
// AddinProject.cs
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
using System.Xml.Serialization;

namespace CydinBuildService
{
	public class AddinProject
	{
		List<AddinProjectSource> rootSources = new List<AddinProjectSource> ();
		List<AddinProjectAddin> addins = new List<AddinProjectAddin> ();
		
		[XmlAttribute ("appVersion")]
		public string AppVersion { get; set; }
		
		[XmlElement ("Project")]
		[XmlElement ("Source")]
		public List<AddinProjectSource> RootSources {
			get {
				return this.rootSources;
			}
		}

		[XmlElement ("Addin")]
		[XmlElement ("AddIn")]
		public List<AddinProjectAddin> Addins {
			get {
				if (addins.Count == 0 && rootSources.Count > 0) {
					AddinProjectAddin a = new AddinProjectAddin ();
					a.Sources = rootSources;
					addins.Add (a);
				}
				return this.addins;
			}
		}
	}
	
	public class AddinProjectSource
	{
		[XmlAttribute ("platforms")]
		public string Platforms { get; set; }
		
		[XmlElement]
		public string AddinFile { get; set; }
		
		[XmlElement]
		public string BuildFile { get; set; }
		
		[XmlElement]
		public string BuildConfiguration { get; set; }
	}
	
	public class AddinProjectAddin
	{
		[XmlElement ("Source")]
		List<AddinProjectSource> sources = new List<AddinProjectSource> ();
		
		public List<AddinProjectSource> Sources {
			get {
				return this.sources;
			}
			set {
				sources = value;
			}
		}
	}
}

