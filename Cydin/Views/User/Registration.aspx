<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<User>" %>
<%@ Import Namespace="Cydin.Models" %>
<%@ Import Namespace="Cydin.Properties" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Registration
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="center">
    <b>We can't find an account associated with this user.</b><br />
    <br /><br />
    
    <% if (!string.IsNullOrEmpty (Settings.Default.PreviousWebSiteHost)) { %>
    <div style="border:solid 1px lightgrey; padding:10px">
    <b>NOTICE </b>
	<p>The domain of this site has recently changed to <b><%=Settings.Default.WebSiteHost%></b>.
	Some OpenId providers may require a validation of the new domain. If you registered an user
	account when the site was using the old domain, click <a href="http://<%=Settings.Default.PreviousWebSiteHost%>/User/IdUpdateLogin?ticket=<%=HttpUtility.UrlEncode (ViewData["ticket"] as string)%>">here</a> to update it.</p>
	</div>
    <br /><br />
    <% } %>
    
    <b>To create an account tied to this OpenID, please fill out the following fields:</b><br />
    
    <%= Html.ValidationSummary("Please correct the errors and try again.") %>
    <% using (Html.BeginForm ()) {
           User user = (User)Model; %>
        <p>
            <label for="Name">Username:</label><br />
            <%= Html.TextBox ("Login", user.Login, new { @class = "form-text" })%>
            <%= Html.ValidationMessage ("Login", "*")%>
        </p>
        <p>
            <label for="Email">Email:</label><br />
            <%= Html.TextBox("Email", user.Email, string.IsNullOrEmpty (user.Email) ? (object) new { @class = "form-text" } : (object) new { @class = "form-text", @readonly="readonly" }) %>
            <%= Html.ValidationMessage("Email", "*") %>
        </p>
        <p>
            <input type="submit" value="Create Account" />
        </p>
    <% } %>
    
    <% Html.RenderPartial ("BlueBox", "If you want to associate this OpenID with an existing Community Add-in Repository account, this will not do it.  That functionality is not available yet."); %>
</div>
</asp:Content>
