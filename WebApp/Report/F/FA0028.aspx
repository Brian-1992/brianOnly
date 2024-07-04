<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FA0028.aspx.cs" Inherits="WebApp.Report.F.FA0028" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">        
        <asp:HiddenField ID="_token" runat="server"/>
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
            <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Height="100%" Width="100%" SizeToReportContent="True">
                <LocalReport ReportPath="Report\F\FA0028.rdlc"></LocalReport>
            </rsweb:ReportViewer>
        </div>

    </form>
</body>
</html>
