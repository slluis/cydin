<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Cydin.Models.Project>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Upload Release
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Upload Release</h2>

    <div class="editor-label">
       <%= Html.Label("Select file to upload:") %>
    </div>
    <% using (Html.BeginForm ("UploadReleaseFile", "Project", new { projectId = Model.Id }, FormMethod.Post, new { enctype = "multipart/form-data" })) { %>

    <input type="file" name="file" />
       
    <p>
        <input type="submit" value="Upload" />
    </p>

    <% } %>

</asp:Content>
