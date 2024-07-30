using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIS14SUPDETtransfer
{
    public class MI_WINVMON
    {
        public string DATA_YM { get; set; }         //	年月
        public string WH_NO { get; set; }           //	庫房代碼
        public string MMCODE { get; set; }          //	院內碼
        public string INV_QTY { get; set; }         //	本月庫存數量
        public string APL_INQTY { get; set; }           //	入庫總量
        public string APL_OUTQTY { get; set; }          //	撥發總量
        public string TRN_INQTY { get; set; }           //	調撥入總量
        public string TRN_OUTQTY { get; set; }          //	調撥出總量
        public string ADJ_INQTY { get; set; }           //	調帳入總量
        public string ADJ_OUTQTY { get; set; }          //	調帳出總量
        public string BAK_INQTY { get; set; }           //	繳回入庫總量
        public string BAK_OUTQTY { get; set; }          //	繳回出庫總量
        public string REJ_OUTQTY { get; set; }          //	退貨總量
        public string DIS_OUTQTY { get; set; }          //	報廢總量
        public string EXG_INQTY { get; set; }           //	換貨入庫總量
        public string EXG_OUTQTY { get; set; }          //	換貨出庫總量
        public string MIL_INQTY { get; set; }           //	戰備換入
        public string MIL_OUTQTY { get; set; }          //	戰備換出
        public string INVENTORYQTY { get; set; }            //	盤點差異量
        public string TUNEAMOUNT { get; set; }          //	軍品調帳金額 
        public string USE_QTY { get; set; }         //	耗用總量
        public string TURNOVER { get; set; }            //	週轉率
        public string SAFE_QTY { get; set; }            //	安全量
        public string OPER_QTY { get; set; }            //	作業量
        public string SHIP_QTY { get; set; }            //	運補量
        public string DAVG_USEQTY { get; set; }         //	日平均消耗量
        public string ONWAY_QTY { get; set; }           //	在途數量
        public string SAFE_DAY { get; set; }            //	安全日
        public string OPER_DAY { get; set; }            //	作業日
        public string SHIP_DAY { get; set; }            //	運補日
        public string HIGH_QTY { get; set; }            //	基準量
        public string ORI_INV_QTY { get; set; }         //	原始結存數量(盤點異動前)
        public string ORI_USE_QTY { get; set; }         //	原始消耗數量(盤點異動前)

        public string STRDATE2 { get; set; }
        public string STRDEPID { get; set; }
        public string STRDRUGID { get; set; }
        public string LNGNOWREST { get; set; }
    }
}
