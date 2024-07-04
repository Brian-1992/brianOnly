using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AA0106M : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string INV_QTY { get; set; }
        public string EX_INV_QTY { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string MMNAME_C { get; set; }        //中文品名
        public string MMNAME_E { get; set; }        //英文品名
        public string MMNAME { get; set; }
        public string MAT_CLASS { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_CONTID { get; set; }
        public string MIL { get; set; }
        public string DATA_YM { get; set; }
        public string M_AGENNO { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string INV_QTY_INCR { get; set; }
        public string INV_QTY_DECR { get; set; }
        public string WH_NO { get; set; } //單位代碼",
        public string WH_NAME { get; set; } //單位名稱",
        public string SUM1 { get; set; } //上月結存金額",
        public string SUM2 { get; set; } //本月進貨金額",
        public string SUM3 { get; set; } //本月消耗金額",
        public string SUMTOT { get; set; } //本月結存金額",
        public string SUM4 { get; set; } //本月盤盈虧金額",
        public string RAT { get; set; } //期末比值",
    }
}