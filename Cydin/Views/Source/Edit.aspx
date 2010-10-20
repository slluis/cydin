<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Cydin.Models.VcsSource>" %>
 
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Edit
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<script type="text/javascript">
 $(document).ready(function() {
  updateSelection ();
  $("#Type").change(function () {
    updateSelection ();
  });
 });
 
function updateSelection ()
{
  $("#col-tags").css("display","none");
  $("#col-branches").css("display","none");
  var sel = $("#Type").attr("value");
  if (sel == "GIT") {
	  $("#col-tags").css("display",null);
	  $("#col-branches").css("display",null);
  }
  if (sel == "BZR") {
	  $("#col-tags").css("display",null);
  }
  $("#help-SVN").css("display","none");
  $("#help-GIT").css("display","none");
  $("#help-BZR").css("display","none");
  $("#help-" + sel).css("display",null);
}
</script>
<h2>
    <% bool initialCreation = (bool)ViewData["CreatingProject"];
       if (initialCreation)
           Response.Write ("Source Code");
       else if ((bool)ViewData["Creating"])
           Response.Write ("New Source");
       else
           Response.Write ("Edit Source");%>
</h2>

    <p>Specify the location of the version control repository where the project source code has to be pulled from. The specified location must follow some rules:</p>
	<ul>
		<li>The root directory must have a file named <b>addin-project.xml</b>, with the content specified <%= Html.ActionLink("here", "AddinProjectHelp", new { projectId=Model.ProjectId }, new { target="_blank" }) %>.</li>
		<li>There must be a <b>MSbuild solution</b> or project file, which will be used to build the project.</li>
	</ul>

    <p>Enter the repository location:</p>
    <% using (Html.BeginForm ((bool)ViewData["Creating"] ? "Create" : "Edit", "Source", new { initialCreation = initialCreation })) {%>

        <%= Html.ValidationSummary (true)%>
        
            <table>
            <tr>
            <tr><td><b>System</b></td>
            <td>
                <%
        var vcsList = new SelectListItem[] {
                        new SelectListItem () { Text="Subversion", Value="SVN" },
                        new SelectListItem () { Text="Git", Value="GIT" },
                        new SelectListItem () { Text="Bazaar", Value="BZR" }
                    };
        foreach (var it in vcsList) it.Selected = Model.Type == it.Value;
                 %>
                <%= Html.DropDownListFor (model => model.Type, vcsList)%>
                <%= Html.ValidationMessageFor (model => model.Type)%>
            </td>
            </tr>
            <tr>
            <td><b>URL</b></td>
            <td>
                <%= Html.TextBoxFor (model => model.Url, new { style = "width:400px" })%>
                <%= Html.ValidationMessageFor (model => model.Url)%>
            </td>
            </tr>
            <tr>
            <td><b>Directory</b></td>
            <td>
                <%= Html.TextBoxFor (model => model.Directory, new { style = "width:350px" })%>
                <%= Html.ValidationMessageFor (model => model.Directory)%>
            </td>
            </tr>
            <tr id="col-tags">
            <td><b>Tags</b></td>
            <td>
                <%= Html.TextBoxFor (model => model.Tags)%>
                <%= Html.ValidationMessageFor (model => model.Tags)%><br>
            </td>
            </tr>
            <tr id="col-branches">
            <td><b>Branches</b></td>
            <td>
                <%= Html.TextBoxFor (model => model.Branches)%>
                <%= Html.ValidationMessageFor (model => model.Branches)%>
            </td>
            </tr>
            </table>

            <br />

			<span id="help-SVN">
            The <b>URL</b> is the address of the source code repository. For example, to import a 'test-project' project hosted in Google you would specify:
            <blockquote> https://test.googlecode.com/svn/trunk/test-project </blockquote>
            You can also import a set of branches or tags at a specified location. For example:
            <blockquote> https://test.googlecode.com/svn/trunk/tags/test-project/* </blockquote>
            When appending '*' to the URL, all branches or tags below the specified location will be imported.
            <p>In the <b>Directory</b> field you can specify a directory relative to the repository root where the addin-project.xml file is located.</p>
            </span>
            
			<span id="help-GIT">
            The <b>URL</b> is the address of the source code repository. For example, to import a 'test-project' project hosted in github you would specify:
            <blockquote> git://github.com/someuser/test-project.git </blockquote>
            <p>In the <b>Directory</b> field you can specify a directory relative to the repository root where the addin-project.xml file is located.</p>
            <p>In the <b>Tags</b> and <b>Branches</b> fields you can specify a comma separated list of tags and branches to import.
            Wildcards are allowed, for example: "v1.0, v1.1, v2.*". If you want to import all branches or tags, specify "*" in the corresponding field.</p>
            </span>
            
			<span id="help-BZR">
            The URL is the address of the source code repository. For example, to import a 'test-project' project hosted in Launchpad you would specify:
            <blockquote> http://bazaar.launchpad.net/~someuser/test-project </blockquote>
            <p>In the <b>Directory</b> field you can specify a directory relative to the repository root where the addin-project.xml file is located.</p>
            <p>In the <b>Tags</b> field you can specify a comma separated list of tags to import.
            Wildcards are allowed, for example: "v1.0, v1.1, v2.*". If you want to import all tags, specify "*" in the corresponding field.</p>
            </span>
            
            <%=Html.HiddenFor (model => model.Id)%>
            <%=Html.HiddenFor (model => model.ProjectId)%>
            
            <br />
            <hr />
            <br />

            <div class="editor-field">
                <%= Html.CheckBoxFor (model => model.AutoPublish)%> Publish releases automatically at every change
                <%= Html.ValidationMessageFor (model => model.AutoPublish)%>
            </div>
            
            <p>
                <input type="submit" value="Save" />
                <% if (initialCreation) { %>
                <input type="button" value="Skip" onclick="window.location.href='/Project/Index/<%=Model.ProjectId %>'"/>
                <%}
                   else { %>
                <input type="button" value="Cancel" onclick="window.location.href='/Source?projectId=<%=Model.ProjectId %>'"/>
                <%} %>
            </p>

    <% } %>

</asp:Content>

