<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Cydin.Models.Application>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Edit Application
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Application</h2>
    <%= Html.ValidationSummary("Edit was unsuccessful. Please correct the errors and try again.") %>
    <% using (Html.BeginForm ("UpdateApplication","SiteAdmin")) { %>
    <table width="600" cellpadding="5px">
        <%= Html.Hidden ("Id", Model.Id) %>
        <tr>
            <td>Name:</td>
            <td>
                <%= Html.TextBox ("Name", Model.Name) %>
                <%= Html.ValidationMessage ("Name", "*") %>
            </td>
        </tr>
        <tr>
            <td>
                Supported Platforms:
            </td>
            <td>
                <%= Html.TextBox ("Platforms", Model.Platforms, new { @class = "form-text", style = "width: 260px;" })%>
                <%= Html.ValidationMessage ("Platforms", "*")%>
            </td>
        </tr>
        <tr>
            <td>
                Subdomain:
            </td>
            <td>
                <%= Html.TextBox ("Subdomain", Model.Subdomain, new { @class = "form-text", style = "width: 260px;" })%>
                <%= Html.ValidationMessage ("Subdomain", "*")%>
            </td>
        </tr>
    </table>
    <br><br>
    <input type="submit" value="Save" />
    <input type="button" value="Cancel" onclick="window.location.href='/SiteAdmin'"/>
    <% } %>
</asp:Content>
