<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="Cydin.Views.UserViewPage" %>
<%@ Import Namespace="Cydin.Builder" %>
<%@ Import Namespace="Cydin.Properties" %>
<%@ Import Namespace="Cydin.Models" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Cydin Setup
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<script type="text/javascript">
		$(function() {
			$("#settings").load("/SiteAdmin/EditSettings");
		});
	</script>
    	<h2>Welcome to Cydin!</h2>
    	<p>Before start using Cydin, you need to specify some configuration parameters. You can change all those settings later on
    	in the Administration page.</p>
    	<div id="settings"></div>
</asp:Content>
