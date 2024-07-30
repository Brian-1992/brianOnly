using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class MI_WINVCTL : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string SAFE_DAY { get; set; }
        public string OPER_DAY { get; set; }
        public string SHIP_DAY { get; set; }
        public int SAFE_QTY { get; set; }
        public int OPER_QTY { get; set; }
        public int SHIP_QTY { get; set; }
        public int DAVG_USEQTY { get; set; }
        public int HIGH_QTY { get; set; }
        public int LOW_QTY { get; set; }
        public int MIN_ORDQTY { get; set; }
        public string SUPPLY_WHNO { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string RESERVEFLAG { get; set; }
        public string NOWCONSUMEFLAG { get; set; }
        public string IS_AUTO { get; set; }
        public string CTDMDCCODE { get; set; }
        public string USEADJ_CLASS { get; set; }
        public string ISSPLIT { get; set; }
        public string DAVG_USEQTY_90 { get; set; }
        public string FSTACKDATE { get; set; }
        public string SAFE_QTY_90 { get; set; }
        public string OPER_QTY_90 { get; set; }
        public string SHIP_QTY_90 { get; set; }
        public string HIGH_QTY_90 { get; set; }

        public string WH_NAME { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string APL_INQTY { get; set; }
        public string DOCNO { get; set; }
        public string APPQTY { get; set; }
        public string AVG_PRICE { get; set; }
        public string GTAPL_REASON { get; set; }

        public string SUGGEST_QTY { get; set; }
        public string INV_QTY { get; set; }


    }
}