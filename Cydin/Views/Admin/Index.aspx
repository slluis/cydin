<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="Cydin.Views.UserViewPage" %>
<%@ Import Namespace="Cydin.Builder" %>
<%@ Import Namespace="Cydin.Properties" %>
<%@ Import Namespace="Cydin.Models" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Administration
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<link href="/Content/jquery.jqplot.css" rel="stylesheet" type="text/css" />
	<script type="text/javascript" src="/Scripts/jquery.cookie.js"></script> 
	<script type="text/javascript" src="/Scripts/stat.query.js"></script> 					
	<script type="text/javascript" src="/Scripts/plot/jquery.jqplot.js"></script>
	<script language="javascript" type="text/javascript" src="/Scripts/plot/plugins/jqplot.dateAxisRenderer.js"></script> 
	<script language="javascript" type="text/javascript" src="/Scripts/plot/plugins/jqplot.canvasAxisTickRenderer.js"></script> 
	<script language="javascript" type="text/javascript" src="/Scripts/plot/plugins/jqplot.canvasTextRenderer.js"></script> 
	<script language="javascript" type="text/javascript" src="/Scripts/plot/plugins/jqplot.highlighter.js"></script> 
	<script language="javascript" type="text/javascript" src="/Scripts/plot/plugins/jqplot.cursor.js"></script>
	<script language="javascript" type="text/javascript" src="/Scripts/plot/plugins/jqplot.pieRenderer.js"></script> 

	<script type="text/javascript">
		$(function() {
			$("#tabs").tabs( {cookie: { expires: 1 } });
		
	    	$(".admins-tr").hover (
				function() { $(this).children("#admin-delete").show(); },
				function() { $(this).children("#admin-delete").hide(); }
			);
		
	    	$(".admin-delete-button").click (function() {
				var uid = $(this).attr("uid");
				$("#admin-name").text ($(this).attr("uname"));
				$("#confirm-delete-admin-dialog").dialog({
					modal: true,
					width: 400,
					resizable: false,
					buttons: {
						Cancel: function() { $(this).dialog("close"); },
						Remove: function() { window.location = getActionUrl("RemoveAdmin","Admin") + "?userId=" + uid; }
					}
				});
			});
		
	    	$("#admin-add-button").click (function() {
				$("#add-admin-dialog").dialog({
					modal: true,
					width: 400,
					resizable: false,
					buttons: {
						Cancel: function() { $(this).dialog("close"); },
						Add: function() { addAdmin ($(this), $("#new-admin-mail").val()); }
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
		
			$.statQueryWidget("stat-query", 
				[ getActionUrl ("GetRepoDownloadStatsAsync","Admin") + "?", 
		          getActionUrl ("GetDownloadStatsAsync","Admin") + "?",
		          getActionUrl ("GetTopDownloads","Admin") + "?"
				],
				function () {
					unplotData ("chartRepos");
					unplotData ("chartAddins");
					$("#topDownloads").fadeTo (200, 0.5);
					$("#errorMessage").hide ();
					$("#loadingMessage").show ();
				},
				function (data, idx) {
					if (idx == 0) {
						plotData ("chartRepos", data, "Repository Index Downloads");
					} else if (idx == 1) {
						plotData ("chartAddins", data, "Add-in Downloads");
					} else {
						showTopDownloads (data);
					}
					$("#loadingMessage").hide ();
				},
				function () {
					$("#loadingMessage").hide ();
					$("#errorMessage").show ();
				}
			);
		})
			
		function deleteRelease (id)
		{
			window.location = getActionUrl("Delete","AppRelease") + "?id=" + id;
		}
		
		function unplotData (chart)
		{
			$("#" + chart).fadeTo (200, 0.5);
			$("#" + chart + "Pie").fadeTo (200, 0.5);
		}
		
		function plotData (chart, data, title)
		{
			$("#" + chart).html ("");
	
			$.jqplot.config.enablePlugins = true;
			var values = [];
			var series = [];
			for (var n=0;n<data.series.length;n++) {
				values[n] = data.series[n].values;
				series[n] = {label:data.series[n].name};
				if (data.series[n].name == "Total")
					series[n].lineWidth=1;
			}
			plot1 = $.jqplot(chart, values, {
			       legend: {show: true, location: 'nw'},
				   title: title,
			       series: series,
			       axes: {
			               xaxis: {renderer:$.jqplot.DateAxisRenderer,
			                       rendererOptions:{tickRenderer:$.jqplot.CanvasAxisTickRenderer},
			                       tickOptions:{
			                               formatString:'%b %#d, %Y',
			                               fontSize:'10pt',
			                               angle:-30
			                       }
			               },
				           yaxis: {
				               tickOptions: {
				                   formatString: '%d'
				               }
				           }
				   },
					highlighter: {
						sizeAdjust: 10,
						tooltipSeparator: '',
						tooltipLocation: 'n',
						tooltipAxes: 'y',
						useAxesFormatters: true
					},
					cursor: { show: true }
			});
			$("#" + chart).fadeTo (300, 1);
		
			$("#" + chart + "Pie").html ("");
		    plot2 = $.jqplot(chart + "Pie", [data.totals], {
		        seriesDefaults:{
					renderer:$.jqplot.PieRenderer,
					rendererOptions: {
						showDataLabels: true
					}
 		        },
		        grid: {
    		        drawBorder: false,
		            drawGridlines: false,
		            shadow:false
				},
		        legend: {
					location:"s",
					rendererOptions: {
						numberRows: 1
					},
					show:true
				}
		    });
			$("#" + chart + "Pie").fadeTo (300, 1);
		}
		
		function showTopDownloads (data)
		{
			var htm = "<table><tr><th>Downloads</th><th>Add-in</th><th>Platform</th></tr>";
			for (n=0; n<data.length; n++) {
				var di = data[n];
				htm += "<tr><td align='center'>" + di.count + "</td><td><a href='" + getActionUrl ("Index","Project") + "?id=" + di.projectId + "'>" + di.name + "</a></td><td>" + di.platform + "</td></tr>";
			}
			$("#topDownloads").html (htm);
			$("#topDownloads").fadeTo (300, 1);
		}
		
    
	function addAdmin (dlg, mail)
	{
		$.post (getActionUrl("AddAdminAsync","Admin"), {email: mail}, function(xml) {
			if (xml == "OK")
				window.location = getActionUrl("Index","Admin");
			else
				$("#user-not-found").show ();
		});
	}

	</script>
    <h2>Administration</h2>

	<div id="tabs">
	<ul>
		<li><a href="#tabs-1">Status</a></li>
		<li><a href="#tabs-2">Add-in Repository</a></li>
		<li><a href="#tabs-3">Releases</a></li>
		<li><a href="/Admin/ProjectsList">Projects</a></li>
		<li><a href="#tabs-4">Administrators</a></li>
		<li><a href="#tabs-5">Statistics</a></li>
	</ul>
	<div id="tabs-1">
    <b>Service Address:</b> <%=BuildService.AuthorisedBuildBotConnection %><br>
    <b>Status:</b> <%=BuildService.Status %><br><br>
    </div>
    <div id="tabs-2">
	    <%=Html.ActionLink ("Update Repository", "UpdateRepositories") %>
	</div>

	<div id="tabs-3">
	<p><%=Html.ActionLink ("Add New Release", "Create", "AppRelease", null, new { @class="command" }) %></p>
    <table>
    <tr><th>Release</th><th>Add-in Version</th><th>Compatible With</th><th></th></tr>
    <% 
    UserModel m = CurrentUserModel;
    foreach (AppRelease r in m.GetAppReleases ()) { %>
    <tr>
    <td> <%= m.CurrentApplication.Name + " " + r.AppVersion%> </td>
    <td> <%= r.AddinRootVersion%> </td>
    <td> <%= r.CompatibleAppReleaseId.HasValue ? m.GetAppRelease (r.CompatibleAppReleaseId.Value).AppVersion : "" %> </td>
    <td> <a href="#" class="command delete-release-button" relid="<%=r.Id%>">Delete</a> <%=Html.ActionLink ("Edit", "Edit", "AppRelease", new { id = r.Id }, new { @class="command" }) %> </td>
    </tr>
    <% } %>
    </table>
    </div>
	
	<div id="tabs-4">
	<p>The following users have administration rights:</p>
	<table>
	<%
	var admins = m.GetApplicationAdministrators ();
	foreach (User u in admins) {%>
		<tr class="admins-tr"><td><%=Html.ActionLink (u.Login, "Profile", "User", new { id = u.Id }, null)%></td>
		<% if (admins.Count() > 1 || CurrentUserModel.IsSiteAdmin) { %>
			<td id="admin-delete" style="display:none;white-space:nowrap"><img src="/Media/bullet_delete.png"/> <a class="admin-delete-button" uname="<%=u.Login%>" uid="<%=u.Id%>" href="#" style="font-size:x-small">Remove</a></td>
		<% } %>
		<td width="50px"></td>
		</tr>
	<% } %>
	</table>
	<p><a href="#" id="admin-add-button" class="command">Add Administrator</a></p>
	</div>
    
	<div id="tabs-5">
		<span id="stat-query"></span> 
		<span id="loadingMessage" style="display:none">
		Loading data...
		</span>			
		<span id="errorMessage" style="display:none">
		Data retrieval failed
		</span>
		<table>
		<tr>
		<td><div id="chartRepos" class='plot' style="margin-top:10px; margin-left:10px; width:670px; height:300px;"></div>
		<%= Html.ActionLink("Export to CSV file", "GetRepoDownloadsCSV")%>
		</td>
		<td><div id="chartReposPie" class='plot' style="margin-top:10px; margin-left:10px; width:230px; height:230px;"></div></td>
		</tr><tr>
		<td><div id="chartAddins" class='plot' style="margin-top:10px; margin-left:10px; width:670px; height:300px;"></div>
		<%= Html.ActionLink("Export to CSV file", "GetDownloadsCSV")%>
		</td>
		<td><div id="chartAddinsPie" class='plot' style="margin-top:10px; margin-left:10px; width:230px; height:230px;"></div></td>
		</tr>
		</table>
		<p>Top Downloads:</p>
		<div id="topDownloads"></div>
	</div>
	
	</div>
	
	<div id="confirm-delete-admin-dialog" title="Remove Owner" style="display:none">
		<p>Are you sure you want to remove the user <b><span id="owner-name"></span></b> from the list of project owners?</p>
	</div>
	
	<div id="add-admin-dialog" title="Add Administrator" style="display:none">
		<p>Enter the e-mail of the user you want to add to the administrators list:</p>
		<form>
			<input id="new-admin-mail"></input>
		</form>
		<p id="user-not-found" style="color:red;display:none">There is no user registered with the provided e-mail</p>
	</div>
    
<div id="confirm-delete-release-dialog" title="Remove Application Release" style="display:none">
	<p>Are you sure you want to delete this release?</p>
</div>
</asp:Content>
