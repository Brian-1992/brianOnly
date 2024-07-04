using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class ME_DOCEXP : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string SEQ { get; set; }
        public string EXP_DATE { get; set; }
        public string LOT_NO { get; set; }
        public string MMCODE { get; set; }
        public string APVQTY { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string C_TYPE { get; set; }
        public string C_STATUS { get; set; }
        public string C_RNO { get; set; }
        public string C_AMT { get; set; }
        public string C_UP { get; set; }
        public string ITEM_NOTE { get; set; }
        public string REASON { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_CONTPRICE { get; set; }
        public string INV_QTY { get; set; }
        public string BALANCE { get; set; }
        public string LOT_NO_N { get; set; }
        public string EXP_DATE_T { get; set; }
        public string EXP_DATET { get; set; }
        public string INOUT { get; set; }
        public string INOUT_N { get; set; }
        public DateTime EXPDATE { get; set; }
        public string APVID { get; set; }
        public string MEMO { get; set; }

        public string WARNYM { get; set; }
        public string UPDATE_USER_NAME { get; set; }
        public string ITEM_STRING { get; set; }
        public DateTime ORI_EXPDATE { get; set; }
        public string SUM_EX { get; set; }
        public string SUM_IN { get; set; }
        public string M_AGENNO { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string AGEN_NAMEE { get; set; }
        public string EXPDATET { get; set; }
        public string DOCNO_M { get; set; }
        public string DOCNO_E { get; set; }
        public string MMCODE1 { get; set; }
        public int EXP_QTY { get; set; }
        public float M_DISCPERC { get; set; }
        public string MMCODE_O { get; set; }
        public string LOT_NO_O { get; set; }
        public string MEMO_O { get; set; }
        public string EXP_DATE_O { get; set; }
        public int EXP_QTY_O { get; set; }
        public string IN_PRICE { get; set; }
        public string CONTPRICE { get; set; }
        public string LAST_QTY { get; set; }
    }
}
