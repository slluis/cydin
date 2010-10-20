<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Cydin.Models.Project>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Confirm Delete
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
<p><b>Are you sure you want to delete the project?</b></p>
All project information and all released packages will be removed from the repository.
</p>

<%=Html.ActionLink ("Cancel", "Index", new { id=Model.Id }) %> |
<%=Html.ActionLink ("Delete", "Delete", new { id=Model.Id }) %>
</asp:Content>
