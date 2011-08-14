<%@ Page Title="" Language="C#" Inherits="Cydin.Views.UserViewPage" %>
<%@ Import Namespace="Cydin.Builder" %>
<%@ Import Namespace="Cydin.Properties" %>
<%@ Import Namespace="Cydin.Models" %>

<html xmlns="http://www.w3.org/1999/xhtml">
<body>

<table>
<tr><th>User</th><th>e-mail</th><th>Projects</th></tr>
<% foreach (User u in CurrentServiceModel.GetUsers ()) { %>
<tr>
<td> <%=Html.ActionLink (u.Login, "Profile", "User", u, null)%> </td>
<td> <a href="mailto:<%=u.Email%>"><%=u.Email%></a></td>
<td>
	<% foreach (Project p in CurrentServiceModel.GetUserProjects (u.Id)) { %>
		<%=Html.ActionLink (p.Name, "Index", "Project", new { id = p.Id }, null)%> 
	<% } %>
</td>
</tr>
<% } %>
</table>

<p>Add new user:</p>
<% using (Html.BeginForm ("AddUser", "SiteAdmin")) {%>
Login: <input name="login" type="text" />&nbsp;&nbsp;Password: <input name="password" type="password" />&nbsp;&nbsp;E-mail: <input name="email" type="text" />&nbsp;&nbsp;<input type="submit"/>
<% } %>		
</body>
</html>
