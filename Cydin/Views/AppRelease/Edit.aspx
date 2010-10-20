<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Cydin.Models.AppRelease>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Edit
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Edit</h2>

    <% using (Html.BeginForm ("UploadAssemblies", "AppRelease", FormMethod.Post, new { enctype = "multipart/form-data" })) { %>
        <%= Html.ValidationSummary(true) %>
        
            <%= Html.HiddenFor(model => model.Id) %>
            <div class="editor-label">
                <%= Html.LabelFor(model => model.AppVersion) %>
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(model => model.AppVersion) %>
                <%= Html.ValidationMessageFor(model => model.AppVersion) %>
            </div>
            
            <div class="editor-label">
                <%= Html.LabelFor(model => model.AddinRootVersion) %>
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(model => model.AddinRootVersion) %>
                <%= Html.ValidationMessageFor(model => model.AddinRootVersion) %>
            </div>
            
            <div class="editor-label">
                <%= Html.Label("Assemblies Archive") %>
            </div>
            <div class="editor-field">
                <input type="file" name="file" />
            </div>
            

            <p>
                <input type="submit" value="Save" />
            </p>

    <% } %>

    <div>
        <%= Html.ActionLink("Back to List", "Index") %>
    </div>

</asp:Content>

