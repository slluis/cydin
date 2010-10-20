<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%
    if (Request.IsAuthenticated) {
        Cydin.Models.User CurrentUser = Cydin.Models.UserModel.GetCurrent ().User;
%>
<div class="usertext">
    
    <span style="font-size: .8em"><b><%= Html.Encode (CurrentUser.Login)%></b> | <%= Html.ActionLink("Profile", "Profile", "User") %> | 
    <%= Html.ActionLink("Logout", "Logout", "User") %></span>
</div>
<%
    } else {
%>
<%= Html.ActionLink("Sign in / Create account", "Login", "User") %><%
    }
%>
