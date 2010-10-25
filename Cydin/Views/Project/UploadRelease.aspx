<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="Cydin.Views.UserViewPage<Cydin.Models.Project>" %>

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
    
	<p>Target version:</p>
	<% var appList = new List<SelectListItem> ();
	   foreach (var app in CurrentUserModel.GetAppReleases ())
	   		appList.Add (new SelectListItem () { Text=CurrentUserModel.CurrentApplication.Name + " " + app.AppVersion, Value=app.AppVersion });
	   if (appList.Count > 0)
	   		appList [appList.Count - 1].Selected = true;
	 %>
     <%= Html.DropDownList ("appVersion", appList)%>
    <p>
	<p>Target platforms:</p>
	<% foreach (string p in CurrentUserModel.CurrentApplication.PlatformsList) { %>
		<%= Html.CheckBox ("platform-" + p, true) %>
        <%= Html.Label(p) %><br/>
	<% } %>
    <p>
        <input type="submit" value="Upload" />
    </p>

    <% } %>

</asp:Content>
