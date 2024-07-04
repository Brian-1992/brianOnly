using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AA0063M : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string FLOWID { get; set; }
        public string FRWH { get; set; }
        public string APPTIME { get; set; }
        public string DIS_TIME { get; set; }
        public string dis_qty { get; set; }
        public string APPLY_NOTE { get; set; }
        public string APPQTY { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_CONTPRICE { get; set; }
        public string MAT_CLASS { get; set; } //物料分類(畫面)
        public string M_STOREID { get; set; }
        public string MMCODE { get; set; } //院內碼(畫面)
        public string MMNAME_C { get; set; } //中文品名(畫面)
        public string MMNAME_E { get; set; } //英文品名(畫面)
        //public string BARCODE { get; set; } //條碼資料(畫面)
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string VENDER { get; set; }
        public string SEQ { get; set; }
        public string MAT_CLSNAME { get; set; }
        public string INID_NAME { get; set; }


        public string TOWH { get; set; }
    }
}
