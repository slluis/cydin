<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="Cydin.Views.UserViewPage<Cydin.Models.AppRelease>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Edit Application Version
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Edit</h2>

    <% using (Html.BeginForm ("UploadAssemblies", "AppRelease", FormMethod.Post, new { enctype = "multipart/form-data" })) { %>
        <%= Html.ValidationSummary(true) %>
        
            <%= Html.HiddenFor(model => model.Id) %>
            <div class="editor-label">
                Application Version
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(model => model.AppVersion) %>
                <%= Html.ValidationMessageFor(model => model.AppVersion) %>
            </div>
            
            <div class="editor-label">
                Root Add-in Version
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(model => model.AddinRootVersion) %>
                <%= Html.ValidationMessageFor(model => model.AddinRootVersion) %>
            </div>
            
            <div class="editor-label">
                Backwards Compatible With:
            </div>
            <div class="editor-field">
				<% var appList = new List<SelectListItem> ();
				   appList.Add (new SelectListItem () { Text="None", Value="0" });
				   foreach (var app in CurrentUserModel.GetAppReleases ()) {
				   		SelectListItem item = new SelectListItem () { Text=CurrentUserModel.CurrentApplication.Name + " " + app.AppVersion, Value=app.Id.ToString() };
				   		if (Model.CompatibleAppReleaseId == app.Id)
				   			item.Selected = true;
				   		appList.Add (item);
				   	}
				   	appList [0].Selected = Model.CompatibleAppReleaseId==0;
				 %>
			     <%= Html.DropDownList ("CompatibleAppReleaseId", appList)%>
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

