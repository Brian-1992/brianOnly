using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class ME_UIMAST : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; } //
        public string MMCODE { get; set; } //
        public string MMNAME_E { get; set; } //
        public string PACK_UNIT { get; set; } //
        public string PACK_QTY { get; set; } //
        public string PACK_UNIT0 { get; set; } //
        public string PACK_QTY0 { get; set; } //
        public string PACK_UNIT1 { get; set; } //
        public string PACK_QTY1 { get; set; } //
        public string PACK_UNIT2 { get; set; } //
        public string PACK_QTY2 { get; set; } //
        public string PACK_UNIT3 { get; set; } //
        public string PACK_QTY3 { get; set; } //
        public string PACK_UNIT4 { get; set; } //
        public string PACK_QTY4 { get; set; } //
        public string PACK_UNIT5 { get; set; } //
        public string PACK_QTY5 { get; set; } //
        public string CREATE_TIME { get; set; }//建立日期
        public string CREATE_USER { get; set; }//建立人員
        public string UPDATE_TIME { get; set; }//異動日期
        public string UPDATE_USER { get; set; }//異動人員
        public string UPDATE_IP { get; set; }//異動IP
        public string CTDMDCCODE { get; set; }//異動IP
        public string CTDMDCCODE_N { get; set; }//異動IP

        public string PACK_TIMES { get; set; }

        public string DIFFER { get; set; }

        public string PACK_QTY_ORI { get; set; }
        public string PACK_UNIT_ORI { get; set; }
        public string PACK_TIMES_ORI { get; set; }

        public string TOTAL { get; set; }
    }
   
}