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
	public class PackageUploadHandler : IRouteHandler, IHttpHandler
	{
		public IHttpHandler GetHttpHandler (RequestContext requestContext)
		{
			return this;
		}

		public bool IsReusable
		{
			get { return true; }
		}

		public void ProcessRequest (HttpContext context)
		{
			Cydin.Builder.BuildService.CheckClient ();
			
			string appId = context.Request.Headers ["applicationId"];
			string sourceTagId = context.Request.Headers ["sourceTagId"];
			string fileName = context.Request.Headers ["fileName"];
			
			SourceTag stag;
			using (UserModel m = UserModel.GetAdmin (int.Parse (appId))) {
				stag = m.GetSourceTag (int.Parse (sourceTagId));
			}
			string path = Path.Combine (stag.PackagesPath, fileName);
			
			string dir = Path.GetDirectoryName (path);
			if (!Directory.Exists (dir))
				Directory.CreateDirectory (dir);
			
			context.Request.InputStream.SaveToFile (path);
			context.Response.Write ("OK");
		}
	}
}