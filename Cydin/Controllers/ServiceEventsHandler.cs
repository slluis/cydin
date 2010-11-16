using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.IO;
using Cydin.Builder;
using CydinBuildService;
using Cydin.Models;

namespace Cydin.Controllers
{
	public class ServiceEventsHandler : IRouteHandler, IHttpHandler
	{
		public IHttpHandler GetHttpHandler (RequestContext requestContext)
		{
			return this;
		}

		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest (HttpContext context)
		{
			Cydin.Builder.BuildService.CheckClient ();
			context.Response.Buffer = false;
			context.Response.BufferOutput = false;
			context.Response.Output.WriteLine ("[Connected]");
			Cydin.Builder.BuildService.ConnectEventsStream (context.Response.Output);
			System.Threading.Thread.Sleep (100000);
		}
	}
}