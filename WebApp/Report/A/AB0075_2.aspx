<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AB0075_2.aspx.cs" Inherits="WebApp.Report.A.AB0075_2" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">        
        <asp:HiddenField ID="_token" runat="server"/>
        <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Height="100%" Width="100%" SizeToReportContent="True">
            <LocalReport ReportPath="Report\A\AB0075_2.rdlc"></LocalReport>
        </rsweb:ReportViewer>
    </div>
    </form>
</body>
</html>
