// 
// ViewHelper.cs
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
using Cydin.Models;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Text;

namespace Cydin.Views
{
	public static class ViewHelper
	{
		static Dictionary<int,string> homes = new Dictionary<int, string> ();
		
		public static string GetHomeHtml (int appId)
		{
			string text;
			if (homes.TryGetValue (appId, out text))
				return text;
			
			ServiceModel sm = ServiceModel.GetCurrent ();
			Application app = sm.GetApplication (appId);
			sm.Dispose ();
			
			MarkdownSharp.Markdown md = new MarkdownSharp.Markdown ();
			md.AutoHyperlink = true;
			md.AutoNewLines = true;
			md.BaseHeaderLevel = 1;
			try {
				string res = md.Transform (app.Description);
				var newDict = new Dictionary<int,string> (homes);
				newDict [appId] = res;
				homes = newDict;
				return res;
			} catch (Exception ex) {
				return "Invalid description: " + ex.Message;
			}
		}
		
		public static void ClearCache ()
		{
			homes = new Dictionary<int, string> ();
		}
		
		public static string NotificationList<T> (this HtmlHelper<T> html, string postUrl, IEnumerable<NotificationInfo> nots)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("<div id='notification-summary' postUrl='" + postUrl + "'><p>");
			sb.Append ("<span id='notification-summary-list'></span>");
			sb.Append ("<br><br><a href='#' id='notification-change-button' class='command'>Change Subscriptions</a>");
			sb.Append ("</div></p>");
			
		    sb.Append ("<div id='notification-selector' style='display:none'>");
		    sb.Append ("<p>Select the notifications you want to subscribe to:</p>");
		    sb.Append ("<p>");

			foreach (var not in nots) {
				if (not.IsGroup)
					sb.Append ("<p><b>").Append (not.Name).Append ("</b></p>");
				else {
					sb.Append (html.CheckBox (not.Id, not.Enabled, new {nname=not.Name}));
					sb.Append (not.Name).Append ("<br>");
				}
		    }

		    sb.Append ("</p><p>");
		    sb.Append ("<a href='#' id='notification-done-button' class='command'>Done</a>");
		    sb.Append ("</p></div>");
			return sb.ToString ();
		}
		
		public static string ActionIconLink<T> (this HtmlHelper<T> html, string icon, string label, string actionName, string controllerName, object routeValues)
		{
			return "<img src='/Media/" + icon + "'/> " + html.ActionLink (label, actionName, controllerName, routeValues, new { style="font-size:x-small" });
		}
		
		public static string IconLink<T> (this HtmlHelper<T> html, string icon, string label)
		{
			return "<img src='/Media/" + icon + "'/> <a href='#' style='font-size:x-small'>" + label + "</a>";
		}
	}
	
}

