<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Cydin.Models.User>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Edit Profile
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Your Profile</h2>
    <%= Html.ValidationSummary("Edit was unsuccessful. Please correct the errors and try again.") %>
    <% using (Html.BeginForm ("ProfileSave","User")) { %>
    <table width="600" cellpadding="5px">
        <%= Html.Hidden ("Id", Model.Id) %>
        <%= Html.Hidden ("Login", Model.Login)%>
        <tr>
            <td>Username:</td>
            <td width="100%"> <%= Model.Login %> </td>
        </tr>
        <tr>
            <td>Email:</td>
            <td>
                <%= Html.TextBox ("Email", Model.Email, new { @class = "form-text", style = "width: 260px;" }) %>
                <%= Html.ValidationMessage ("Email", "*") %>
            </td>
        </tr>
        <tr>
            <td>
                Name:
            </td>
            <td>
                <%= Html.TextBox ("Name", Model.Name, new { @class = "form-text", style = "width: 260px;" })%>
                <%= Html.ValidationMessage ("Name", "*")%>
            </td>
        </tr>
    </table>
    <br><br>
    <h3>E-mail Notifications</h3>
        <%= Html.TextBox ("Name", Model.Name, new { @class = "form-text", style = "width: 260px;" })%>
    <input type="submit" value="Save" />
    <% } %>
</asp:Content>
