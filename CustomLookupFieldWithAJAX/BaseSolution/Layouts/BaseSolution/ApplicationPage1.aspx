<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ApplicationPage1.aspx.cs" Inherits="BaseSolution.Layouts.BaseSolution.ApplicationPage1" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <script src="JS/jquery-3.2.1.min.js"></script>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">

    <script>
        //var listId = 'D683087D-D4CF-46F4-8EBB-B93D0CBFA607';

        //var data = { listId: listId };


        $.ajax({
            type: 'POST',
            url: "/_Layouts/15/BaseSolution/ApplicationPage1.aspx/GetFieldsList",
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ listId: 'D43B1DD7-711F-4AA1-A20B-A9B681086C9E' }),
            dataType: 'json',
            success: function (msg) {
                console.log(msg.d);
            },
            error: function (msg) {
                console.log(msg);
            }
        });

       </script>

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Application Page
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
My Application Page
</asp:Content>
