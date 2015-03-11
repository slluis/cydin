<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%
	Cydin.Models.User CurrentUser = null;
    if (Request.IsAuthenticated)
        CurrentUser = Cydin.Models.UserModel.GetCurrent ().User;
%>
<span id="logindisplay">
<% if (CurrentUser != null) {%>
    <b><%= Html.Encode (CurrentUser.Login)%></b> | <%= Html.ActionLink("Profile", "Profile", "User") %> | 
    <%= Html.ActionLink("Logout", "Logout", "User") %>
<% } else {%>
	<%= Html.ActionLink("Sign in / Create account", "Login", "User") %>
<% }%>
</span>
