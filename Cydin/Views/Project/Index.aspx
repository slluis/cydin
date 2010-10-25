<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="Cydin.Views.UserViewPage<Cydin.Models.Project>" %>
<%@ Import Namespace="Cydin.Properties" %>
<%@ Import Namespace="Cydin.Models" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="Cydin.Views" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	<%=Model.Name %>
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<script type="text/javascript" src="/Scripts/notifications.js"></script> 
<script type = "text/javascript">
    $(document).ready(function() {
    	$(".sts-list").change (function() { updateStatus (this); });
    });
    
    function updateStatus (ob)
    {
    	var id = ob.id.substring (4);
    	ob.disabled = true;
    	$.post ("/Project/SetDevStatus/<%=Model.Id%>", {stagId: id, value: ob.value}, function(xml) {
    		ob.disabled = false;
    	});
    }
</script>
	<table>
	<tr>
	<td valign="top" width="75%">
	<h2><%=Model.Name %></h2>
    <%
    MarkdownSharp.Markdown md = new MarkdownSharp.Markdown ();
    md.AutoHyperlink = true;
    md.AutoNewLines = true;
    md.BaseHeaderLevel = 2;
    Response.Write (md.Transform (Model.Description));
    %>

    <%  UserModel m = CurrentUserModel;
        var isProjectAdmin = m.CanManageProject (Model);
        var sources = m.GetProjectSources (Model.Id);
        var sourceTags = m.GetProjectSourceTags (Model.Id);
        var releases = m.GetProjectReleases (Model.Id);
    %>
    
    <% if (isProjectAdmin) {
    	Response.Write (Html.ActionLink ("Edit Name and Description", "Edit", "Project", new { id = Model.Id }, null));
    } %>
    <h2>Releases</h2>
    
    <% if (releases.Any ()) { 
    foreach (var appRel in m.GetAppReleases ().OrderBy (r => r.AppVersion)) {
    	var appReleases = releases.Where (r => r.TargetAppVersion == appRel.AppVersion).OrderBy (r => r.LastChangeTime).Reverse ();
    	if (!appReleases.Any())
    		continue;
    %>
    <h3><%=m.CurrentApplication.Name%> <%=appRel.AppVersion%></h3>
    <table>
    <thead>
    <tr><th>Date</th><th>Release</th><th>Packages</th>
    <% if (isProjectAdmin) { %>
    <th>Downloads</th><th>Status</th><th></th></tr>
    <% } %>
    </thead>
    <tbody>
    <%foreach (var release in appReleases) { %>
    <tr>
    <td><%=release.LastChangeTime.ToShortDateString ()%></td>
    <td><%=release.Version%> (<%=release.DevStatus%>)</td>
    <td>
<!--    	<a href="<%=release.GetInstallerVirtualPath ()%>">Install</a> -->
        <% foreach (var plat in release.PlatformsList) { %>
            <a href="<%=release.GetPublishedVirtualPath (plat)%>"><%=plat%></a>
        <% } %>
    </td>

    <% if (isProjectAdmin) { %>
    <td><%=m.GetDownloadSummary (release)%></td>
    <td><%=release.Status%></td>
    <td><%=Html.ActionLink ("Delete", "DeleteRelease", new { releaseId = release.Id })%></td>
    <% } %>

    </tr>
    <% } %>
    </tbody>
    </table>
    <% } }
       else {/* if (Model.Releases.Any ())*/ %>
       <p>There isn't yet any release.</p>
    <% } %>

    <% if (isProjectAdmin) { %>

    <h2>Sources</h2>

    <% if (sourceTags.Any ()) {
    	List<string> rels = m.GetAppReleases ().OrderBy (r => r.AppVersion).Select (r => r.AppVersion).ToList ();
    	rels.Add ("");
    	bool oneShown = false;
    	foreach (var appRel in rels) {
    		var appSources = sourceTags.Where (st => (st.TargetAppVersion ?? "") == appRel);
    		if (!appSources.Any())
    			continue;
    { %>
    <h3><%=appRel != "" ? m.CurrentApplication.Name + " " + appRel : (oneShown ? "Unknown Target Version" : "")%></h3>
    <table class="tag-source-table">
    <%
    oneShown = true;
    foreach (var source in appSources) { 
    	var stat = source.Status.Replace (" ","");
    	string time = source.BuildDate != DateTime.MinValue ? source.BuildDate.ToShortDateString () + " " + source.BuildDate.ToShortTimeString () : "not yet built";
    	var vcss = m.GetSource (source.SourceId);
    	string srcUrl = VersionControlUtil.GetSourceCodeUrl (vcss.Type, source.Url);
    %>
    <tr>
    <td colspan="4">
    <div style="float:left" class="tag-source-table-name tag-source-table-name-<%=stat%>"><%=source.Name%></div>
    <div style="float:left" class="tag-source-table-status tag-source-table-status-<%=stat%>"><%=source.Status%></div></td>
    </tr>
    
    <tr valign="top" class="tag-source-table-body tag-source-table-body-<%=stat%>">
    <td><%=source.AddinId + " v" + source.AddinVersion%><br>
    </td>
    <td><% if (!source.IsUpload) {%>
	<a href="<%=srcUrl%>">Revision: <%=source.ShortLastRevision%> </a><br>
    <%=Html.ActionLink ("Last build: " + time, "BuildLog", new { id = source.Id }, null)%><br>
	<% } else { %>
		Uploaded: <%=time%>
	<% } %>
    </td><td>
    Dev Status: <%= Html.DropDownListFor (t => source.DevStatus, m.GetDevStatusItems (source.DevStatus), new { id="sts-" + source.Id, @class="sts-list" })%><br>
    Packages: 
        <% foreach (var plat in source.PlatformsList) { %>
            <a href="<%=source.GetVirtualPath (plat)%>"><%=plat%></a>
        <% } %>
    </td>
    <td width="0px" align="right">
        <% if ((source.Status == SourceTagStatus.BuildError || source.Status == SourceTagStatus.FetchError || m.IsSiteAdmin) && !source.IsUpload)
               Response.Write (Html.ActionLink ("Rebuild", "UpdateSource", new { sourceTagId = source.Id }) + "<br>"); %>
        <% if (source.Status == SourceTagStatus.Ready)
               Response.Write (Html.ActionLink ("Publish", "PublishRelease", new { sourceId = source.Id }) + "<br>");
        %>
        <% if (source.IsUpload)
               Response.Write (Html.ActionLink ("Delete", "DeleteUpload", new { sourceId = source.Id }));
        %>
    </td>
    </tr>
    <tr height="15px"></tr>
    <% } }  /* foreach */ %>
    </table>
    <% } } /*if (Model.SourceTags.Any ())*/
       else if (sources.Any ()) {  %>
       No sources found.
    <% }
       else { %>
       No version control sources defined. Click on 'Edit Sources' to define new sources.
    <% } %>
    <% if (sources.Any (s => s.Status == SourceTagStatus.FetchError)) {
           Response.Write ("<p>There were some errors while getting version control information:</p><ul>");
           foreach (var s in sources.Where (vs => vs.Status == SourceTagStatus.FetchError))
               Response.Write ("<li>" + s.Url + ": " + HttpUtility.HtmlEncode (s.ErrorMessage) + "</li>");
           Response.Write ("</ul>");
       }
         %>
	
	<% if (Model.HasFlag (ProjectFlag.AllowPackageUpload)) { %>
    <p><%=Html.ActionLink ("Upload Package", "UploadRelease", new { projectId = Model.Id })%></p>
	<% } %>

    <p><%=Html.ActionLink ("Edit Sources", "Index", "Source", new { projectId = Model.Id }, null)%></p>
    <% } /* if(isProjectAdmin) */ %>
    
    </td>
    <td valign="top" width="250px">
    <% if (m.User != null) { %>
    <div class="side-panel">
    <h1>Track Project</h1>
    <%=Html.NotificationList ("/Project/SetNotification/" + Model.Id, m.GetProjectNotifications (Model.Id)) %>
    </div>
    <% } %>
    
    <% if (isProjectAdmin) { %>
    <div class="side-panel">
    <h1>Administration</h1>
    <p><%=Html.ActionLink ("Delete Project", "ConfirmDelete", "Project", new { id = Model.Id }, null)%></p>
    <% if (m.IsAdmin)
         using (Html.BeginForm ("UpdateFlags", "Project")) {%>
	 	<%=Html.Hidden ("projectId", Model.Id)%>
    <p><%=Html.CheckBox ("allowDirectPublish", Model.HasFlag (ProjectFlag.AllowDirectPublish))%> Allow direct publish<br/>
    <%=Html.CheckBox ("allowPackageUpload", Model.HasFlag (ProjectFlag.AllowPackageUpload))%> Allow package upload</p>
     <input type="submit" value="Save" />
    <% } %>
    </div>
    <% } /* if(isProjectAdmin) */ %>
    
    </td>
    </table>
</asp:Content>
