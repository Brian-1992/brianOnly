using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class MI_WINVMON
    {
        public string DATA_YM { get; set; }
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string INV_QTY { get; set; }
        public string APL_INQTY { get; set; }
        public string APL_OUTQTY { get; set; }
        public string TRN_INQTY { get; set; }
        public string TRN_OUTQTY { get; set; }
        public string ADJ_INQTY { get; set; }
        public string ADJ_OUTQTY { get; set; }
        public string BAK_INQTY { get; set; }
        public string BAK_OUTQTY { get; set; }
        public string REJ_OUTQTY { get; set; }
        public string DIS_OUTQTY { get; set; }
        public string EXG_INQTY { get; set; }
        public string EXG_OUTQTY { get; set; }
        public string MIL_INQTY { get; set; }
        public string MIL_OUTQTY { get; set; }
        public string INVENTORYQTY { get; set; }
        public string TUNEAMOUNT { get; set; }
        public string USE_QTY { get; set; }
        public string TURNOVER { get; set; }
        public string SAFE_QTY { get; set; }
        public string OPER_QTY { get; set; }
        public string SHIP_QTY { get; set; }
        public string DAVG_USEQTY { get; set; }
        public string ONWAY_QTY { get; set; }
        public string SAFE_DAY { get; set; }
        public string OPER_DAY { get; set; }
        public string SHIP_DAY { get; set; }
        public string HIGH_QTY { get; set; }
        public string ORI_INV_QTY { get; set; }
        public string ORI_USE_QTY { get; set; }

        public MI_WINVMON()
        {
            DATA_YM = string.Empty;
            WH_NO = string.Empty;
            MMCODE = string.Empty;
            INV_QTY = "0";
            APL_INQTY = "0";
            APL_OUTQTY = "0";
            TRN_INQTY = "0";
            TRN_OUTQTY = "0";
            ADJ_INQTY = "0";
            ADJ_OUTQTY = "0";
            BAK_INQTY = "0";
            BAK_OUTQTY = "0";
            REJ_OUTQTY = "0";
            DIS_OUTQTY = "0";
            EXG_INQTY = "0";
            EXG_OUTQTY = "0";
            MIL_INQTY = "0";
            MIL_OUTQTY = "0";
            INVENTORYQTY = "0";
            TUNEAMOUNT = "0";
            USE_QTY = "0";
            TURNOVER = "0";
            SAFE_QTY = "0";
            OPER_QTY = "0";
            SHIP_QTY = "0";
            DAVG_USEQTY = "0";
            ONWAY_QTY = "0";
            SAFE_DAY = "0";
            OPER_DAY = "0";
            SHIP_DAY = "0";
            HIGH_QTY = "0";
        }
    }
}