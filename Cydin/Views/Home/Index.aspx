<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="Cydin.Views.UserViewPage" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="Cydin.Properties" %>
<%@ Import Namespace="Cydin.Models" %>
<%@ Import Namespace="Cydin.Views" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%=CurrentUserModel.CurrentApplication.Name + " Community Add-in Repository"%>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<script type="text/javascript" src="/Scripts/notifications.js"></script> 

    <%  UserModel m = CurrentUserModel; %>
    
    <table width="100%" border="0" cellspacing="0" cellpadding="0" id="content-table">
    <tr valign="top"><td width="75%">
    <%=Cydin.Views.ViewHelper.GetHomeHtml (m.CurrentApplication.Id)%>
    <% if (m.IsAdmin) {
    	Response.Write (Html.ActionLink ("Edit Page", "Edit", "Home"));
    } %>
    <h2>Recent Releases</h2>
    <%  foreach (Release rel in m.GetRecentReleases ()) {%>
    <b><%=Html.ActionLink (m.GetProject (rel.ProjectId).Name + " v" + rel.Version + " (" + rel.DevStatus + ")", "Index", "Project", new {id=rel.ProjectId}, null) %></b><br>
    <%=rel.LastChangeTime.ToLongDateString ()%>, for <%=m.CurrentApplication.Name%> <%=rel.TargetAppVersion%>
    <br>
    <br>
    <% } %>
    </td>

    <td>
    <% if (m.User != null) { %>
    <div id="user-projects" class="side-panel">
    <h1>My Projects</h1>
    <% if (m.GetUserProjects ().Any ()) {
           Response.Write ("<ul>");
           foreach (Project p in m.GetUserProjects ()) { %>
            <li><%=Html.ActionLink (p.Name, "Index", "Project", new { id = p.Id }, null)%></li>
        <% }
           Response.Write ("</ul>");
       }
       else { %>
       <p>There are no projects.</p>
       <% } %>

    <%= Html.ActionLink ("Create a new Project", "Create", "Project")%>
    </div>
    <div id="user-notifications" class="side-panel">
    <h1>My Notifications</h1>
    <%=Html.NotificationList ("/Home/SetApplicationNotification", m.GetApplicationNotifications ()) %>
    </div>
    <%} %>
    </td>
    </tr>
    </table>
</asp:Content>
