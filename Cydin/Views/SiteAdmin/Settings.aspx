<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Cydin.Properties.Settings>" %>
<%@ Import Namespace="Cydin.Properties" %>

<html xmlns="http://www.w3.org/1999/xhtml">
<body>
	<script type="text/javascript">
		 $(document).ready(function() {
			$("#operation-selector").change(function () {
				updateOperation();
			});
			updateOperation ();
		});
		
		function updateOperation ()
		{
			var sel = $("#operation-selector").attr("value");
			if (sel == "1")
				$("#operation-selector-desc").text ("The site provides add-ins for a single application.");
			else if (sel == "2")
				$("#operation-selector-desc").html ("Multiple applications are supported. Each application is identified by a subdomain in the url, for example: <b>monodevelop</b>.cydin.net");
			else if (sel == "3")
				$("#operation-selector-desc").html ("Multiple applications are supported. Each application is identified by a path in the url, for example: cydin.net/<b>monodevelop</b>");
		}
	</script>
	<% 
        var modeList = new SelectListItem[] {
                    new SelectListItem () { Text="Single Application", Value="1" },
                    new SelectListItem () { Text="Multiple Application Subdomains", Value="2" },
                    new SelectListItem () { Text="Multiple Application Paths", Value="3" },
                };
        foreach (var it in modeList)
        	it.Selected = Settings.Default.OperationMode == (OperationMode) int.Parse (it.Value);
		
	%>
    <% using (Html.BeginForm ("SaveSettings", "SiteAdmin")) { %>
        <%= Html.ValidationSummary(true) %>
            <br>
            <b>Operation Mode</b>
            <div class="editor-field">
                <%= Html.DropDownListFor (model => model.OperationMode, modeList, new { id="operation-selector" })%>
                <p id="operation-selector-desc"></p>
            </div>
            
            <b>Web Site Host</b>
            <div class="editor-field">
                <%= Html.TextBoxFor(model => model.WebSiteHost) %>
                <%= Html.ValidationMessageFor(model => model.WebSiteUrl) %>
            </div>
            <p>Host name of this web site, to be used in e-mail notifications. For example, addins.monodevelop.com</p>
        
            <b>Data Files Path</b>
            <div class="editor-field">
                <%= Html.TextBoxFor(model => model.DataPath) %>
                <%= Html.ValidationMessageFor(model => model.DataPath) %>
            </div>
            <p>Path to a directory in the server where built files will be stored. This path doesn't need to be in the web site directory. Web user must have write access to it.</p>
            
            <br>
            <hr>
            <h3>SMTP Configuration</h3>
            <br>
            <b>SMTP Host</b>
            <div class="editor-field">
                <%= Html.TextBoxFor(model => model.SmtpHost) %>
                <%= Html.ValidationMessageFor(model => model.SmtpHost) %>
            </div>
            <br>
            <b>SMTP Port</b>
            <div class="editor-field">
                <%= Html.TextBoxFor(model => model.SmtpPort) %>
                <%= Html.ValidationMessageFor(model => model.SmtpPort) %>
            </div>
            <br>
            <b>Use SSL</b>
            <div class="editor-field">
                <%= Html.CheckBoxFor(model => model.SmtpUseSSL) %>
                <%= Html.ValidationMessageFor(model => model.SmtpUseSSL) %>
            </div>
            <br>
            <b>SMTP User</b>
            <div class="editor-field">
                <%= Html.TextBoxFor(model => model.SmtpUser) %>
                <%= Html.ValidationMessageFor(model => model.SmtpUser) %>
            </div>
            <br>
            <b>SMTP Password</b>
            <div class="editor-field">
                <%= Html.PasswordFor(model => model.SmtpPassword) %>
                <%= Html.ValidationMessageFor(model => model.SmtpPassword) %>
            </div>
            
            <p>
                <input type="submit" value="Save" />
    <% if (!Settings.Default.InitialConfiguration) { %>
                <input type="button" value="Cancel" onclick="window.location.href='/'"/>
    <% } %>
            </p>

    <% } %>

</body>
</html>

