<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="Cydin.Views.UserViewPage" %>
<%@ Import Namespace="Cydin.Properties" %>
<%@ Import Namespace="Cydin.Models" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Release Review
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Release Review</h2>

    <table>
    <tr>
    <th>Date</th><th>Project</th><th>Addin Version</th><th><%=CurrentUserModel.CurrentApplication.Name %> Version</th><th></th>
    </tr>
    <% foreach (Release rel in CurrentUserModel.GetPendingReleases (ReleaseStatus.PendingReview)) { %>
    <tr>
    <td><%=rel.LastChangeTime %></td>
    <td><%=Html.ActionLink (CurrentUserModel.GetProject (rel.ProjectId).Name, "Index", "Project", new {id=rel.ProjectId}, null) %></td>
    <td><%=rel.Version %></td>
    <td><%=rel.TargetAppVersion %></td>
    <td>
    <%=Html.ActionLink ("Approve", "ApproveRelease", new { id = rel.Id })%> | 
    <%=Html.ActionLink ("Reject", "RejectRelease", new { id = rel.Id })%> | 
    </td>
    </tr>
    <% } %>
    </table>

</asp:Content>
