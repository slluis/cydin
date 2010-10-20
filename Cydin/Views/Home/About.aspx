<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Cydin.Properties" %>
<%@ Import Namespace="Cydin.Models" %>

<asp:Content ID="aboutTitle" ContentPlaceHolderID="TitleContent" runat="server">
    About Us
</asp:Content>

<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>About</h2>
    <% var m = UserModel.GetCurrent ();
    if (m.CurrentApplication != null) { %>
    <p>This web site is a Community Add-in Repository for <%=m.CurrentApplication.Name %>.</p>

	<p>Using this web site, add-in developers can publish their <%=m.CurrentApplication.Name %> add-ins
	 and make them available to all users. The site works like a build bot: it pulls the source code of add-ins from
	 version control repositories, builds and packages them, and then publishes them in an add-in repository,
	 to which <%=m.CurrentApplication.Name %> is subscribed.</p>
    </p>
    
    <p>This site is based on Cydin v<%=Settings.CydinVersion %>.</p>
    <% } %>
</asp:Content>
