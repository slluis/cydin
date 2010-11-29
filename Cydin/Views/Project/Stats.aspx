<%@ Page Language="C#" Inherits="Cydin.Views.UserViewPage<Cydin.Models.DownloadStats>"%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>
<head runat="server">
	<title>Stats</title>
	<script type="text/javascript" src="/Scripts/plot/jquery.jqplot.js"></script>
	<script language="javascript" type="text/javascript" src="/Scripts/plot/plugins/jqplot.dateAxisRenderer.js"></script> 
	<script language="javascript" type="text/javascript" src="/Scripts/plot/plugins/jqplot.canvasAxisTickRenderer.js"></script> 
	<script language="javascript" type="text/javascript" src="/Scripts/plot/plugins/jqplot.canvasTextRenderer.js"></script> 
	<script language="javascript" type="text/javascript" src="/Scripts/plot/plugins/jqplot.highlighter.js"></script> 
	<script language="javascript" type="text/javascript" src="/Scripts/plot/plugins/jqplot.cursor.js"></script> 					
	<script type="text/javascript" src="/Scripts/stat.query.js"></script> 
<script type = "text/javascript">

var currentPeriod = "last";
var currentArg = "30d";				
var projectId = <%=ViewData["projectId"]%>;
var releaseId = <%=ViewData["releaseId"]%>;

$.statQueryWidget("stat-query", 
	[getActionUrl ("GetStatsAsync","Project") + "?pid=" + projectId + "&relid=" + releaseId + "&"],
	function () {
		$("#chart").hide ();
		$("#chart").html ("");
		$("#errorMessage").hide ();
		$("#loadingMessage").show ();
	},
	function (data) {
		$("#chart").show ();
		$("#loadingMessage").hide ();
		plotData (data.series);
	},
	function () {
		$("#loadingMessage").hide ();
		$("#errorMessage").show ();
	}
);

function plotData (data)
{
		$.jqplot.config.enablePlugins = true;
		var values = [];
		var series = [];
		for (var n=0;n<data.length;n++) {
			values[n] = data[n].values;
			series[n] = {label:data[n].name}
		}
		plot1 = $.jqplot("chart", values, {
		       legend: {show: true, location: 'nw'},
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
}
				
</script>
</head>
<body>
<div id="stat-query"></div> 
<div id="loadingMessage" style="display:none">
<p>Loading data...</p>
</div>			
<div id="errorMessage" style="display:none">
<p>Data retrieval failed</p>
</div>	
<div id="chart" class='plot' style="margin-top:10px; margin-left:10px; width:740px; height:420px;"></div> 
</body>
</html>
