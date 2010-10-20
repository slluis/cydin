<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<Cydin.Models.ServiceModel>" %>
<%@ Import Namespace="Cydin.Builder" %>
<%@ Import Namespace="Cydin.Properties" %>
<%@ Import Namespace="Cydin.Models" %>

<html xmlns="http://www.w3.org/1999/xhtml">
<body>

<table>
<tr><th>User</th><th>e-mail</th><th>Projects</th></tr>
<% foreach (User u in Model.GetUsers ()) { %>
<tr>
<td> <%=Html.ActionLink (u.Login, "Profile", "User", u, null)%> </td>
<td> <a href="mailto:<%=u.Email%>"><%=u.Email%></a></td>
<td>
	<% foreach (Project p in Model.GetUserProjects (u.Id)) { %>
		<%=Html.ActionLink (p.Name, "Index", "Project", new { id = p.Id }, null)%> 
	<% } %>
</td>
</tr>
<% } %>
</table>
</body>
</html>
