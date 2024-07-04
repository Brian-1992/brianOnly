using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using System.Data;

namespace WebApp.Report.A
{
    public partial class GetPDF : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            /*======================參考資料======================
             * 不用ReportViewer 控制項, 直接由程式呼叫報表檔(.rdlc)來輸出報表
             http://gowintony.blogspot.com/2018/12/cnet-reportview-rdlc.html
             瀏覽器直接開啟pdf檔，不出現詢問下載視窗
             https://dotblogs.com.tw/tsxz2009/2013/05/01/102536
             ======================參考資料======================*/

            //Rdlc路徑
            string StrPath = "";
            if (Page.Session["RDLC_Path"] != null)
                StrPath = Server.MapPath(Convert.ToString(Page.Session["RDLC_Path"]));

            string StrDataTableName = "";
            //DataTable名稱，要跟資料集DataTable相同
            if (Page.Session["DataTableName"] != null)
                StrDataTableName = Convert.ToString(Page.Session["DataTableName"]);

            Dictionary<string, string> Dict1 = null;
            var parDict1 = Page.Session["Dict1"];
            //ReportParameter
            if (parDict1 != null)
                Dict1 = (Dictionary<string, string>)parDict1;
            //產生PDF的資料
            DataTable tmpdt_DTTable = null;
            if (Page.Session["DTTable"] != null)
                tmpdt_DTTable = (DataTable)Page.Session["DTTable"];
            //輸出格式
            string StrPrtType = "PDF";
            RenderReport(StrPath, StrDataTableName, tmpdt_DTTable, Dict1, StrPrtType);
        }

        private void RenderReport(string StrPath, string StrDataTableName, DataTable DTData, Dictionary<string, string> DicParam, string StrPrtType)
        {
            //宣告本機報表物件
            LocalReport LRpt1 = new LocalReport();
            //設定報表檔案路徑
            LRpt1.ReportPath = StrPath;

            //加入報表資料來源
            LRpt1.DataSources.Add(new ReportDataSource(StrDataTableName, DTData));

            //報表上是否能輸出影像
            LRpt1.EnableExternalImages = true;

            //傳遞報表參數
            ReportParameter[] RParam = new ReportParameter[DicParam.Count];

            int i = 0;
            foreach (KeyValuePair<string, string> element in DicParam)
            {
                RParam[i] = new ReportParameter(element.Key, element.Value);
                i++;
            }

            LRpt1.SetParameters(RParam);

            //Render (out 參數)
            string StrMime = "";       //(報表的 MIME 類型 ex:如果輸出為PDF,則會傳回 "application/pdf")
            string StrEncode = "";
            string StrExten = "";        //(輸出檔所用的副檔名 ex 如果輸出為PDF,則會傳回 "pdf")
            string[] StrStreamArray;
            Warning[] OWar;            //(紀錄 產生報表時發生的錯誤訊息)


            //產生的位元組陣列
            byte[] ByteArray = LRpt1.Render(StrPrtType, null, out StrMime, out StrEncode, out StrExten, out StrStreamArray, out OWar);

            //設定輸出的資料流 Http MIME 類型 (ex: "application/pdf")
            HttpContext.Current.Response.ContentType = StrMime;

            //HTML <head>
            //設定輸出的檔案名稱
            string FileName = "File." + StrExten;
            //attachment會強制下載，inline直接在瀏覽器顯示
            //HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + FileName);
            HttpContext.Current.Response.AddHeader("Content-Disposition", "inline; filename=" + FileName);

            //設定以 位元組陣列 表示的請求的長度(指的是<body>要輸出的長度)
            HttpContext.Current.Response.AddHeader("Content-Length", ByteArray.Length.ToString());

            //設定 Http MIME 類型 為任意的二進位數據(準備開始輸出到<body>的位元組陣列)
            //octet-stream會強制下載，Application/PDF直接在瀏覽器顯示
            //HttpContext.Current.Response.ContentType = "application/octet-stream";
            HttpContext.Current.Response.ContentType = "Application/PDF";
            HttpContext.Current.Response.BinaryWrite(ByteArray);
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
        }
    }
}