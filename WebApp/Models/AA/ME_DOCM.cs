using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class ME_DOCM : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string DOCTYPE { get; set; }
        public string FLOWID { get; set; }
        public string APPID { get; set; }
        public string APPNAME { get; set; }         // 自訂 人員的中文姓名
        public string APPDEPT { get; set; }
        public string APPDEPTNAME { get; set; }     // 自訂 部門的中名稱
        public string APPTIME { get; set; }
        public string USEID { get; set; }
        public string USEDEPT { get; set; }
        public string USEDEPTNAME { get; set; }     // 自訂 使用部門的中名稱
        public string FRWH { get; set; }
        public string FRWHNAME { get; set; }        // 自訂 核撥庫房的中名稱
        public string TOWH { get; set; }
        public string TOWHNAME { get; set; }        // 自訂 領用庫房的中名稱
        public string PURCHASENO { get; set; }
        public string SUPPLYNO { get; set; }
        public string APPLY_KIND { get; set; }
        public string MAT_CLASS { get; set; }
        public string APPLY_NOTE { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}