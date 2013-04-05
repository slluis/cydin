<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="Cydin.Views.UserViewPage<Cydin.Models.Project>" %>
<%@ Import Namespace="Cydin.Properties" %>
<%@ Import Namespace="Cydin.Models" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="Cydin.Views" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	<%=Model.Name %>
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<link href="/Content/jquery.jqplot.css" rel="stylesheet" type="text/css" />
<script type="text/javascript" src="/Scripts/notifications.js"></script> 
<script type = "text/javascript">
	var pid=<%=Model.Id%>;			
    $(document).ready(function() {
    	$(".sts-list").change (function() { updateStatus (this); });
    	$(".owners-tr").hover (
			function() { $(this).children("#owners-delete").show(); },
			function() { $(this).children("#owners-delete").hide(); }
		);
    	$(".owners-delete-button").click (function() {
			var uid = $(this).attr("uid");
			$("#owner-name").text ($(this).attr("uname"));
			$("#confirm-delete-owner-dialog").dialog({
				modal: true,
				width: 400,
				resizable: false,
				buttons: {
					Cancel: function() { $(this).dialog("close"); },
					Remove: function() { window.location = getActionUrl("RemoveOwner","Project") + "?id=" + pid + "&userId=" + uid; }
				}
			});
		});
    	$("#owners-add-button").click (function() {
			$("#add-owner-dialog").dialog({
				modal: true,
				width: 400,
				resizable: false,
				buttons: {
					Cancel: function() { $(this).dialog("close"); },
					Add: function() { addOwner ($(this), $("#new-owner-mail").val()); }
				}
			});
		});
    	$(".delete-release-button").click (function() {
			var relid = $(this).attr("relid");
			$("#confirm-delete-release-dialog").dialog({
				modal: true,
				width: 400,
				resizable: false,
				buttons: {
					Cancel: function() { $(this).dialog("close"); },
					Delete: function() { deleteRelease (relid); }
				}
			});
		});
    	$(".release-stats-button").click (function() {
			var relid = $(this).attr("relid");
			showStats(relid);
		});
    });
    
	function addOwner (dlg, mail)
	{
		$.post (getActionUrl("AddOwnerAsync","Project"), {id: pid, email: mail}, function(xml) {
			if (xml == "OK")
				window.location = getActionUrl("Index","Project") + "/" + pid;
			else
				$("#user-not-found").show ();
		});
	}
			
	function deleteRelease (id)
	{
		window.location = getActionUrl("DeleteRelease","Project") + "?releaseId=" + id;
	}
			
    function updateStatus (ob)
    {
    	var id = ob.id.substring (4);
    	ob.disabled = true;
    	$.post (getActionUrl("SetDevStatus","Project") + "/<%=Model.Id%>", {stagId: id, value: ob.value}, function(xml) {
    		ob.disabled = false;
    	});
    }
			
	function showStats (relid)
	{
		$("#stats-dialog-content").html("Loading...");
		$("#stats-dialog-content").load (getActionUrl ("Stats","Project") + "?projectId=" + pid + "&releaseId=" + relid);
		$("#stats-dialog").dialog({
			modal: true,
			resizable: true,
			width:800,
			height:600,
			buttons: {
				Close: function() { $(this).dialog("close"); }
			}
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
    	Response.Write (Html.ActionLink ("Edit Name and Description", "Edit", "Project", new { id = Model.Id }, new { @class = "command" }));
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
    <%
	foreach (var release in appReleases) { %>
    <tr>
    <td><%=release.LastChangeTime.ToShortDateString ()%></td>
    <td><%=release.AddinId%> <%=release.Version%> (<%=release.DevStatus%>)</td>
    <td>
<!--    	<a href="<%=release.GetInstallerVirtualPath ()%>">Install</a> -->
        <% foreach (var plat in release.PlatformsList) { %>
            <a href="<%=release.GetPublishedVirtualPath (plat)%>"><%=plat%></a>
        <% } %>
    </td>

    <% if (isProjectAdmin) { %>
    <td><a href="#" class="release-stats-button" relid="<%=release.Id%>"><img src="/Media/chart_bar.png"/> <%=m.Stats.GetDownloadSummary (release)%></a></td>
    <td><%=release.Status%></td>
	<td><a href="#" class="delete-release-button command" relid="<%=release.Id%>">Delete</a></td>
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
    <td><%=source.AddinId + " v" + source.AddinVersion%><br/>
    </td>
    <td><% if (!source.IsUpload) {%>
	<a href="<%=srcUrl%>">Revision: <%=source.ShortLastRevision%> </a><br/>
    <%=Html.ActionLink ("Last build: " + time, "BuildLog", new { id = source.Id }, null)%><br/>
	<% } else { %>
		Uploaded: <%=time%>
	<% } %>
    </td><td>
    Dev Status: <%= Html.DropDownListFor (t => source.DevStatus, m.GetDevStatusItems (source.DevStatus), new { id="sts-" + source.Id, @class="sts-list" })%><br/>
    Packages: 
        <% foreach (var plat in source.PlatformsList) { %>
            <a href="<%=source.GetVirtualPath (plat)%>"><%=plat%></a>
        <% } %>
    </td>
    <td width="0px" align="right">
        <% if ((source.Status == SourceTagStatus.BuildError || source.Status == SourceTagStatus.FetchError || m.IsSiteAdmin) && !source.IsUpload)
               Response.Write (Html.ActionLink ("Rebuild", "UpdateSource", new { sourceTagId = source.Id }, new { @class="command" }) + "<br>"); %>
        <% if (source.Status == SourceTagStatus.Ready)
               Response.Write (Html.ActionLink ("Publish", "PublishRelease", new { sourceId = source.Id }, new { @class="command" }) + "<br>");
        %>
        <% if (source.IsUpload)
               Response.Write (Html.ActionLink ("Delete", "DeleteUpload", new { sourceId = source.Id }, new { @class="command" }));
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
	
	<p>
	<% if (Model.HasFlag (ProjectFlag.AllowPackageUpload)) { %>
    <%=Html.ActionLink ("Upload Package", "UploadRelease", new { projectId = Model.Id }, new { @class="command" })%>
	<% } %>

    <%=Html.ActionLink ("Edit Sources", "Index", "Source", new { projectId = Model.Id }, new { @class="command" })%>
    <% } /* if(isProjectAdmin) */ %>
    </p>
    </td>
    <td valign="top" width="250px">
    <% if (isProjectAdmin) { %>
    <div class="side-panel">
    <h1>Project Owners</h1>
	<table width="100%">
	<%
	var owners = m.GetProjectOwners (Model);
	foreach (User u in owners) {%>
		<tr class="owners-tr"><td><%=Html.ActionLink (u.Login, "Profile", "User", new { id = u.Id }, null)%></td>
		<% if (owners.Count() > 1) { %>
			<td id="owners-delete" style="display:none" ><img src="/Media/bullet_delete.png"/> <a class="owners-delete-button" uname="<%=u.Login%>" uid="<%=u.Id%>" href="#" style="font-size:x-small">Remove</a></td>
		<% } %>
		</tr>
	<% } %>
	</table>
	<a href="#" id="owners-add-button" class="command">Add</a>
    </div>
    <% } %>
    <% if (m.User != null) { %>
    <div class="side-panel">
    <h1>Track Project</h1>
    <%=Html.NotificationList (GetActionUrl ("SetNotification","Project") + "/" + Model.Id, m.GetProjectNotifications (Model.Id)) %>
    </div>
    <% } %>
    
    <% if (isProjectAdmin) { %>
    <div class="side-panel">
    <h1>Administration</h1>
    <p><%=Html.ActionLink ("Delete Project", "ConfirmDelete", "Project", new { id = Model.Id }, new {@class="command"})%></p>
    <% if (m.IsAdmin)
         using (Html.BeginForm ("UpdateFlags", "Project")) {%>
	 	<%=Html.Hidden ("projectId", Model.Id)%>
    <p><%=Html.CheckBox ("allowDirectPublish", Model.HasFlag (ProjectFlag.AllowDirectPublish))%> Allow direct publish<br/>
    <%=Html.CheckBox ("allowPackageUpload", Model.HasFlag (ProjectFlag.AllowPackageUpload))%> Allow package upload</p>
     <input type="submit" value="Save"  class="command"/>
    <% } %>
    </div>
    <% } /* if(isProjectAdmin) */ %>
    
    </td>
    </table>
		
<div id="confirm-delete-release-dialog" title="Remove Owner" style="display:none">
	<p>Are you sure you want to delete this release?</p>
</div>
<div id="confirm-delete-owner-dialog" title="Remove Owner" style="display:none">
	<p>Are you sure you want to remove the user <b><span id="owner-name"></span></b> from the list of project owners?</p>
</div>

<div id="add-owner-dialog" title="Add Owner" style="display:none">
	<p>Enter the e-mail of the user you want to add to the owners list:</p>
	<form>
		<input id="new-owner-mail"></input>
	</form>
	<p id="user-not-found" style="color:red;display:none">There is no user registered with the provided e-mail</p>
</div>
		
<div id="stats-dialog" title="Download Statistics" style="display:none">
	<div id="stats-dialog-content">
			tt
	</div>
</div>
		
</asp:Content>
