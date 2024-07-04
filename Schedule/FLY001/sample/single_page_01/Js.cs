using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GenTools.sample.single_page_01
{


    public class Js
    {
        // ---------------------
        // -- 共用函式區 開始 --
        // ---------------------
        public class 查詢條件項目
        {
            public enum 元件類型 { textbox, textarea, combo, date_picker, month_picker, radios, checks, file, displayfield }
            public String label_name;   // 標籤
            public 元件類型 kind;
            public String combo_sql;    // combo(動態讀資料庫) 資料來源的sql
            public List<KeyValuePair<String, String>> lstComboItems = new List<KeyValuePair<string, string>>(); // combo(靜態)

            public List<KeyValuePair<String, String>> lstRadioItems = new List<KeyValuePair<string, string>>(); // 


        } // ec 查詢條件項目

        string 初始化組態物件(GenTools.Form1 form1)
        {
            查詢條件項目 qc;

            qc = new 查詢條件項目();                  // 01
            qc.label_name = "庫房別";
            qc.kind = 查詢條件項目.元件類型.combo;
            qc.combo_sql = "select 'a' texts, 'b' values from dual where 1=1 ";
            lst查詢條件包.Add(qc);

            qc = new 查詢條件項目();                  // 02
            qc.label_name = "年月";
            qc.kind = 查詢條件項目.元件類型.month_picker;
            lst查詢條件包.Add(qc);

            qc = new 查詢條件項目();                  // 03
            qc.label_name = "";
            qc.kind = 查詢條件項目.元件類型.radios;
            KeyValuePair<String, String> kvp;
            kvp = new KeyValuePair<string, string>("庫備品", "0");
            qc.lstRadioItems.Add(kvp);
            kvp = new KeyValuePair<string, string>("非庫備品(排除鎖E品項)", "1");
            qc.lstRadioItems.Add(kvp);
            kvp = new KeyValuePair<string, string>("庫備品(管控項目)", "2");
            qc.lstRadioItems.Add(kvp);
            lst查詢條件包.Add(qc);
            string json = JsonConvert.SerializeObject(this);
            return json;
        }

        // ---------------------
        // -- 共用函式區 結束 --
        // ---------------------

        String getHeader()
        {
            String s = "";
            s += "Ext.Loader.setConfig({" + sBr;
            s += "    enabled: true," + sBr;
            s += "    paths: {" + sBr;
            s += "        'WEBAPP': '/Scripts/app'" + sBr;
            s += "    }" + sBr;
            s += "});" + sBr;
            s += "" + sBr;
            s += "Ext.require(['WEBAPP.utils.Common']);" + sBr;
            s += "" + sBr;
            return s;
        } // 

        String getBody()
        {
            String s = "" + sBr;
            s += "Ext.onReady(function () { " + sBr;
            s += "});" + sBr;
            return s;
        }
        


        public List<查詢條件項目> lst查詢條件包 = new List<查詢條件項目>();
        public bool had匯出功能 = true;
        public bool had列印功能 = true;
        String sBr = "\r\n";

        public String getCode(GenTools.Form1 form1)
        {
            String json = 初始化組態物件(form1);
            Js js = JsonConvert.DeserializeObject<Js>(json); // 解回產程式碼的物件
            String sCode = "";

            sCode += getHeader();
            sCode += getBody();
            return sCode;
        }

    } // ec Js


    


} // en
