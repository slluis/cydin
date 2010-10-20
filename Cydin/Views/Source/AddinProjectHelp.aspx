<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Cydin.Properties" %>
<%@ Import Namespace="Cydin.Models" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Index
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>addin-project.xml Format</h2>

	<p>Every add-in must have a project configuration file named <b>addin-project.xml</b> at the root directory of the source code tree.</p>
	<p>Here is an example of the content of the file:</p>
	<pre>
&lt;AddinProject appVersion="2.4"&gt;
	&lt;Project platforms="Mac Linux"&gt;
		&lt;AddinFile&gt;SimpleAddin/bin/Debug/SimpleAddin.dll&lt;/AddinFile&gt;
		&lt;BuildFile&gt;SimpleAddin.sln&lt;/BuildFile&gt;
		&lt;BuildConfiguration&gt;Debug&lt;/BuildConfiguration&gt;
	&lt;/Project&gt;
	&lt;Project platforms="Win32"&gt;
		&lt;AddinFile&gt;SimpleAddin/bin/DebugWin32/SimpleAddin.dll&lt;/AddinFile&gt;
		&lt;BuildFile&gt;SimpleAddin.sln&lt;/BuildFile&gt;
		&lt;BuildConfiguration&gt;DebugWin32&lt;/BuildConfiguration&gt;
	&lt;/Project&gt;
&lt;/AddinProject&gt;
	</pre>

Comments:
<ul>
<li><b>AddinProject</b> is the root element. It must have an attribute named <b>appVersion</b> which specifies the <%=UserModel.GetCurrent().CurrentApplication.Name %> version that the add-in is targetting.</li>
<li>There must be at least one <b>Project</b> element with the following child elements and attributes:
<ul><li><b>platforms</b> attribute (optional): names of the platforms that this add-in configuration is targetting. It can be Linux, Mac or Win32.
  It is possible to specify several values separated by a space. If no value is specified, the configuration applies to all platforms.</li>
<li><b>AddinFile</b> element: relative path to the add-in assembly, or to the xml manifest if it's not embedded as resource.</li>
<li><b>BuildFile</b> element (optional): relative path to the MSBuild project or solution file to use to build the add-in.</li>
<li><b>BuildConfiguration</b> element (optional): name of the build configuration to use to build the add-in.</li>
</li>
</ul>

</asp:Content>

