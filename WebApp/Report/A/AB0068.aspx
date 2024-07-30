<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AB0068.aspx.cs" Inherits="WebApp.Report.A.AB0068" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>
</head>


<body>
    <form id="form1" runat="server">        
        <asp:HiddenField ID="_token" runat="server"/>
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
            <rsweb:reportviewer id="ReportViewer1" name="ReportViewer1" runat="server" font-names="Verdana" font-size="8pt" waitmessagefont-names="Verdana" waitmessagefont-size="14pt" height="100%" width="100%" sizetoreportcontent="True">
            <LocalReport ReportPath="Report\A\AB0068.rdlc"></LocalReport>
        </rsweb:reportviewer>
        </div>
    </form>
</body>
</html>
