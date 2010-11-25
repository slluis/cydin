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
		
	    	$(".admins-tr").hover (
				function() { $(this).children("#admin-delete").show(); },
				function() { $(this).children("#admin-delete").hide(); }
			);
		
	    	$(".admin-delete-button").click (function() {
				var uid = $(this).attr("uid");
				$("#admin-name").text ($(this).attr("uname"));
				$("#confirm-delete-admin-dialog").dialog({
					modal: true,
					width: 400,
					resizable: false,
					buttons: {
						Cancel: function() { $(this).dialog("close"); },
						Remove: function() { window.location = getActionUrl("RemoveAdmin","Admin") + "?userId=" + uid; }
					}
				});
			});
		
	    	$("#admin-add-button").click (function() {
				$("#add-admin-dialog").dialog({
					modal: true,
					width: 400,
					resizable: false,
					buttons: {
						Cancel: function() { $(this).dialog("close"); },
						Add: function() { addAdmin ($(this), $("#new-admin-mail").val()); }
					}
				});
			});
		})
		
    
	function addAdmin (dlg, mail)
	{
		$.post (getActionUrl("AddAdminAsync","Admin"), {email: mail}, function(xml) {
			if (xml == "OK")
				window.location = getActionUrl("Index","Admin");
			else
				$("#user-not-found").show ();
		});
	}

	</script>
    <h2>Administration</h2>

	<div id="tabs">
	<ul>
		<li><a href="#tabs-1">Status</a></li>
		<li><a href="#tabs-2">Add-in Repository</a></li>
		<li><a href="#tabs-3">Releases</a></li>
		<li><a href="/Admin/ProjectsList">Projects</a></li>
		<li><a href="#tabs-4">Administrators</a></li>
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
	
	<div id="tabs-4">
	<p>The following users have administration rights:</p>
	<table>
	<%
	var admins = m.GetApplicationAdministrators ();
	foreach (User u in admins) {%>
		<tr class="admins-tr"><td><%=Html.ActionLink (u.Login, "Profile", "User", new { id = u.Id }, null)%></td>
		<% if (admins.Count() > 1 || CurrentUserModel.IsSiteAdmin) { %>
			<td id="admin-delete" style="display:none;white-space:nowrap"><img src="/Media/bullet_delete.png"/> <a class="admin-delete-button" uname="<%=u.Login%>" uid="<%=u.Id%>" href="#" style="font-size:x-small">Remove</a></td>
		<% } %>
		<td width="50px"></td>
		</tr>
	<% } %>
	</table>
	<p><a href="#" id="admin-add-button" class="command">Add Administrator</a></p>
	</div>
    
	</div>
	
	<div id="confirm-delete-admin-dialog" title="Remove Owner" style="display:none">
		<p>Are you sure you want to remove the user <b><span id="owner-name"></span></b> from the list of project owners?</p>
	</div>
	
	<div id="add-admin-dialog" title="Add Administrator" style="display:none">
		<p>Enter the e-mail of the user you want to add to the administrators list:</p>
		<form>
			<input id="new-admin-mail"></input>
		</form>
		<p id="user-not-found" style="color:red;display:none">There is no user registered with the provided e-mail</p>
	</div>
    
</asp:Content>
