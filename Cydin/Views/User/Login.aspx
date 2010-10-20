<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="Cydin.Views.UserViewPage" %>
<%@ Import Namespace="Cydin.Properties" %>
<%@ Import Namespace="Cydin.Models" %>

<asp:Content ID="Content3" ContentPlaceHolderID="TitleContent" runat="server">
	Log in
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <% if (ViewData["Message"] != null) { %>
    <div style="border: solid 1px red">
        <%= Html.Encode(ViewData["Message"].ToString())%>
    </div>
    <% } %>
	<% if (Settings.Default.InitialConfiguration && !CurrentServiceModel.ThereIsAdministrator ()) {%>
	<h2>Site Administrator Account</h2>
	<p>An administrator account needs to be created. To create the account, click on your OpenID provider:</p>
	<% } else {%>
    <p>
    To log in, or to register an account, click on your OpenID provider:
    </p>
    <% } %>
    	<table cellspacing="5px" style="border-collapse: separate">
    	<tr>
    	<td style="border:solid lightgrey 1px;padding:15px">
        <a id="OIDgoogle" style="cursor:pointer" onclick="document.getElementById('openid_identifier').value = 'https://www.google.com/accounts/o8/id';document.getElementById('openid_submit').click()" style="cursor:hand"><img border=""0 src="/Media/signin-google.png" alt="Google" /></a>
    	</td>
    	<td style="border:solid lightgrey 1px;padding:15px">
        <a id="OIDyahoo" style="cursor:pointer"  onclick="document.getElementById('openid_identifier').value = 'http://yahoo.com/';document.getElementById('openid_submit').click()"><img border="0" src="/Media/signin-yahoo.png" alt="Yahoo!" /></a>
    	</td>
    	<td style="border:solid lightgrey 1px;padding:15px">
        <a id="OIDmyopenid" style="cursor:pointer"  onclick="document.getElementById('openid_identifier').value = 'http://myopenid.com/';document.getElementById('openid_submit').click()"><img border="0" src="/Media/signin-myopenid.png" alt="myOpenID" /></a>
    	</td>
        </tr>
        </table>
    <p>
        Or specify another OpenID provider:</p>
    
    <p>
    <form action="Authenticate?ReturnUrl=<%=HttpUtility.UrlEncode(Request.QueryString["ReturnUrl"]) + "&ticket=" + HttpUtility.UrlEncode (ViewData["ticket"] as string) %>" method="post">
        <label for="openid_identifier">OpenID:</label>
        <input id="openid_identifier" name="openid_identifier" size="40" />
        <input id="openid_submit" type="submit" value="Login" />
    </form>
    </p>
    <p>If you don't yey have an OpenID account, <a href="https://www.myopenid.com/signup?affiliate_id=59653">click here to signup</a>.</p>
    
    <script type="text/javascript">
        document.getElementById("openid_identifier").focus();
    </script>
</asp:Content>
