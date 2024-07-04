<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AA0147.aspx.cs" Inherits="WebApp.Report.AA.AA0147" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>

    <script>
        function PrintPdf() {
            var MyFrame = document.getElementById('iframe_PrintReport');
            MyFrame.focus();
            MyFrame.contentWindow.print();
        }
    </script>
</head>


<body>
    <iframe id="iframe_PrintReport" name="iframe_PrintReport" style="display: none" runat="server"></iframe>
    <form id="form1" runat="server">        
        <asp:HiddenField ID="_token" runat="server"/>
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
            <rsweb:reportviewer id="ReportViewer1" name="ReportViewer1" runat="server" EnableTelemetry="false" font-names="Verdana" font-size="8pt" waitmessagefont-names="Verdana" waitmessagefont-size="14pt" height="100%" width="100%" sizetoreportcontent="True" >
            <LocalReport ReportPath="Report\A\AA0147.rdlc"></LocalReport>
        </rsweb:reportviewer>
        </div>
    </form>
</body>
</html>
