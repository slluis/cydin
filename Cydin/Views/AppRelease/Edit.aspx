<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="Cydin.Views.UserViewPage<Cydin.Models.AppRelease>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Application Release
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<link href="/Content/jquery.jqplot.css" rel="stylesheet" type="text/css" />
	<h2>
    <% if ((bool)ViewData["Creating"])
           Response.Write ("New Application Release");
       else
           Response.Write ("Edit Application Release");%>
    </h2>

    <% using (Html.BeginForm ((bool)ViewData["Creating"] ? "CreateRelease" : "UpdateRelease", "AppRelease", FormMethod.Post, new { enctype = "multipart/form-data" })) { %>
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
				   appList.Add (new SelectListItem () { Text="None", Value="" });
				   foreach (var app in CurrentUserModel.GetAppReleases ()) {
				   		if (app.Id == Model.Id)
				   			continue;
				   		SelectListItem item = new SelectListItem () { Text=CurrentUserModel.CurrentApplication.Name + " " + app.AppVersion, Value=app.Id.ToString() };
				   		if (Model.CompatibleAppReleaseId == app.Id)
				   			item.Selected = true;
				   		appList.Add (item);
				   	}
				   	appList [0].Selected = !Model.CompatibleAppReleaseId.HasValue;
				 %>
			     <%= Html.DropDownList ("CompatibleAppReleaseId", appList)%>
            </div>
		
            <div class="editor-label">
                <%= Html.Label("Assemblies Archive") %>
            </div>
            <div class="editor-field">
                <input type="file" name="file" />
            </div>
            
            <div class="editor-label">
                Url to Assemblies Archive
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(model => model.ZipUrl) %>
                <%= Html.ValidationMessageFor(model => model.ZipUrl) %>
            </div>

		<br/>
		<hr/>
            <p>
                <input type="submit" value="Save" class="command"/>
		        <%= Html.ActionLink("Cancel", "Index", "Admin", null, new { @class="command" }) %>
            </p>

    <% } %>

    <div>
    </div>

</asp:Content>

