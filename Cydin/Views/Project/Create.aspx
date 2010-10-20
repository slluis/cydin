<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Cydin.Models.Project>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	<%= (bool)ViewData ["Creating"] ? "New Project" : "Edit Project" %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

	<% bool creating = (bool)ViewData ["Creating"]; %>
	
    <h2><%= creating ? "New Project" : Model.Name%></h2>
    

    <% using (Html.BeginForm (creating ? "Create" : "Edit", "Project")) {%>
        <%= Html.ValidationSummary(true) %>

            <div class="editor-label">
                <%= Html.LabelFor(model => model.Name) %>
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(model => model.Name) %>
                <%= Html.ValidationMessageFor(model => model.Name) %>
            </div>
            
            <div class="editor-label">
                Description<br>
            </div>
            <div class="editor-field">
                <%= Html.TextAreaFor (model => model.Description, new { style="width:900px;height:300px" })%>
                <%= Html.ValidationMessageFor(model => model.Description) %><br>
                <small>You can use <a href="http://daringfireball.net/projects/markdown">Markdown</a> syntax. You'll find a <a href="http://stackoverflow.com/editing-help">short syntax guide in StackOverflow</a>. Full <a href="http://daringfireball.net/projects/markdown/syntax">syntax reference</a> in the official page.</small>
            </div>
            
            <p>
            <% if (creating) { %>
                <input type="submit" value="Create" />
            <% } else { %> 
	            <%=Html.HiddenFor (model => model.Id)%>
	            <%=Html.HiddenFor (model => model.ApplicationId)%>
                <input type="submit" value="Save" />
            <% } %>
                <input type="button" value="Cancel" onclick="window.location.href='/'"/>
            </p>

    <% } %>

</asp:Content>

