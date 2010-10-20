<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" ValidateRequest="false" %>
<%@ Import Namespace="Cydin.Properties" %>
<%@ Import Namespace="Cydin.Models" %>

<asp:Content ID="aboutTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Edit Home Page
</asp:Content>

<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
    <p>Home page content:</p>
	
	<% Application app = UserModel.GetCurrent ().CurrentApplication; %>


    <% using (Html.BeginForm ("Update", "Home")) {%>
        <div class="editor-field">
            <%= Html.TextArea ("content", app.Description, new { style="width:900px;height:300px" })%>
            <br/><small>You can use <a href="http://daringfireball.net/projects/markdown">Markdown</a> syntax. You'll find a <a href="http://stackoverflow.com/editing-help">short syntax guide in StackOverflow</a>. Full <a href="http://daringfireball.net/projects/markdown/syntax">syntax reference</a> in the official page.</small>
        </div>
        <input type="submit" value="Save" />
        <input type="button" value="Cancel" onclick="window.location.href='/'"/>
    <% } %>
</asp:Content>
