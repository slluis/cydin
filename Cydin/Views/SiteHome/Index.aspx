<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="Cydin.Views.UserViewPage" %>
<%@ Import Namespace="Cydin.Properties" %>
<%@ Import Namespace="Cydin.Models" %>
<%@ Import Namespace="Cydin.Controllers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Cydin
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%  UserModel m = CurrentUserModel; %>
    
    <table width="100%" border="0" cellspacing="0" cellpadding="0">
    <tr valign="top"><td width="75%">
    <p><b>Welcome to Cydin!</b></p>
    
    <p>Cydin is a add-in repository for applications based on Mono.Addins.</p>
    <%  foreach (Application app in m.GetApplications ()) {%>
    <a href="<%=ControllerHelper.GetActionUrl (app.Subdomain, null, null)%>"><%=app.Name%></a>
    <br>
    <% } %>
    </td>

    <td>
    </td>
    </tr>
    </table>
</asp:Content>
