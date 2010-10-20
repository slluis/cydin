<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<Cydin.Models.ServiceModel>" %>
<%@ Import Namespace="Cydin.Builder" %>
<%@ Import Namespace="Cydin.Properties" %>
<%@ Import Namespace="Cydin.Models" %>

<html xmlns="http://www.w3.org/1999/xhtml">
<body>
<%=Html.Encode (System.IO.File.ReadAllText (BuildService.LogFile)).Replace ("\n","<br/>")%>
</body>
</html>
