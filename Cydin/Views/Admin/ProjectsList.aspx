<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<Cydin.Models.UserModel>" %>
<%@ Import Namespace="Cydin.Builder" %>
<%@ Import Namespace="Cydin.Properties" %>
<%@ Import Namespace="Cydin.Models" %>

<html xmlns="http://www.w3.org/1999/xhtml">
<body>

<table>
<tr><th>Project</th><th>Owner</th><th>Sources</th><th>Releases</th><th>Downloads</th></tr>
<% foreach (Project p in Model.GetProjects ()) { %>
<tr>
<td><%=Html.ActionLink (p.Name, "Index", "Project", new { id = p.Id }, null)%></td>
<td>
	<% foreach (User u in Model.GetProjectOwners (p)) { %>
		<%=Html.ActionLink (u.Login, "Profile", "User", u, null)%> 
	<% } %>
</td>
<td>
<% var tags = Model.GetProjectSourceTags (p.Id); 
   var errs = tags.Where (t => t.Status == SourceTagStatus.BuildError).Count ();
%>
<%=tags.Count ()%>
<%=errs > 0 ? "(" + errs + " errors)" : ""%>
</td>
<td>
<% var rels = Model.GetProjectReleases (p.Id); 
	var count = rels.Count ();
%>
<%= count %>
<% if (count > 0) {%>
(<%
var grp = from rel in rels group rel by rel.TargetAppVersion into g select new {AppVersion=g.Key, Rels=g.Count()};
bool f = true;
foreach (var g in grp) {
	if (!f)
		Response.Write (", ");
	Response.Write (g.AppVersion + "=" + g.Rels);
	f = false;
}
%>)
<% } %>
</td>
<td>
<%=Model.GetDownloadSummary (p)%>
</td>
</tr>
<% } %>
</table>
</body>
</html>
