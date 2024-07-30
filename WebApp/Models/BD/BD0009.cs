using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BD0009 : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string PURDATE { get; set; }
        public string INV_QTY { get; set; }
        public string APL_OUTQTY { get; set; }
        public string APL_INQTY { get; set; }
        public string SAFE_QTY { get; set; }
        public string OPER_QTY { get; set; }
        public string SHIP_QTY { get; set; }
        public string HIGH_QTY { get; set; }
        public string ALLQTY { get; set; }
        public string ESTQTY { get; set; }
        public string ADVISEQTY { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string AGEN_NAME { get; set; }
        public string M_PURUN { get; set; }
        public string CONTRACNO { get; set; }
        public string UNIT_SWAP { get; set; }
        public string PURTYPE { get; set; }
        public string E_PURTYPE { get; set; }
        public string AGEN_NO { get; set; }
        public string M_DISCPERC { get; set; }
        public string PO_PRICE { get; set; }
        public string PO_QTY { get; set; }
        public string PO_AMT { get; set; }
        public string ISTRAN_1 { get; set; }
        public string ISTRAN { get; set; }
        public string MEMO { get; set; }
        public string FLAG { get; set; }
        public string TODAYFLAG { get; set; }
        public string E_RESTRICTCODE { get; set; }
        public string E_VACCINE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        // ============================================================
        public string TP { get; set; }
        public string INWAY_QTY { get; set; }
        public string MIN_ORDQTY { get; set; }
        public string NEWFLAG { get; set; }
        public string CHGFLAG { get; set; }
        // ============================================================
        public string LOW_QTY { get; set; }
        public string SUMINQTY { get; set; }
        public string PACK_QTY0 { get; set; }
        public string CALC { get; set; }
        public string ADVQTY_OLD { get; set; }
        public string E_MANUFACT { get; set; }
        public string DISC_CPRICE { get; set; }

        // ============================================================
        public string USEQTY { get; set; }
        public string BACKQTY { get; set; }

        public string CREATE_TIME { get; set; }
        public string UPDATE_TIME { get; set; }
        // ============================================================
        public string ISWILLING { get; set; }
        public string DISCOUNT_QTY { get; set; }
        public string DISC_COST_UPRICE { get; set; }
    }
}