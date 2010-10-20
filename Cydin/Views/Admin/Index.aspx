<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="Cydin.Views.UserViewPage" %>
<%@ Import Namespace="Cydin.Builder" %>
<%@ Import Namespace="Cydin.Properties" %>
<%@ Import Namespace="Cydin.Models" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Administration
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<script type="text/javascript" src="/Scripts/jquery.cookie.js"></script> 
	<script type="text/javascript">
		$(function() {
			$("#tabs").tabs( {cookie: { expires: 1 } });
		});
	</script>
    <h2>Administration</h2>

	<div id="tabs">
	<ul>
		<li><a href="#tabs-1">Status</a></li>
		<li><a href="#tabs-2">Add-in Repository</a></li>
		<li><a href="#tabs-3">Releases</a></li>
		<li><a href="/Admin/ProjectsList">Projects</a></li>
	</ul>
	<div id="tabs-1">
    <b>Service Address:</b> <%=BuildService.AuthorisedBuildBotConnection %><br>
    <b>Status:</b> <%=BuildService.Status %><br><br>
    </div>
    <div id="tabs-2">
	    <%=Html.ActionLink ("Update Repository", "UpdateRepositories") %>
	</div>

	<div id="tabs-3">
    <table>
    <tr><th>Release</th><th></th></tr>
    <% 
    UserModel m = CurrentUserModel;
    foreach (AppRelease r in m.GetAppReleases ()) { %>
    <tr>
    <td> <%= m.CurrentApplication.Name + " " + r.AppVersion%> </td>
    <td> <%=Html.ActionLink ("Delete", "Delete", "AppRelease", new { id = r.Id }, null) %> <%=Html.ActionLink ("Edit", "Edit", "AppRelease", new { id = r.Id }, null) %> </td>
    </tr>
    <% } %>
    </table>
    </div>
    
	</div>
    
</asp:Content>
